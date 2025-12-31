using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class Form1 : Form
    {
        #region Fields

        private int counter = 0;
        private string[] images;
        private bool playing = false;
        private MusicBeeApiInterface mbApi;
        private PluginConfig config;
        private string currentImagePath;
        private string currentPdfPath;
        private bool hasPdfInCollection = false;

        private ViewMode currentView = ViewMode.Scans;

        private enum ViewMode
        {
            Scans,
            Booklet
        }

        #endregion

        #region Windows API Imports for Form Dragging

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        #endregion

        #region Constructor

        public Form1(MusicBeeApiInterface apiInterface, PluginConfig pluginConfig)
        {
            mbApi = apiInterface;
            config = pluginConfig;

            InitializeComponent();

            this.Size = new Size(config.WindowWidth, config.WindowHeight);
            this.Text = "Album Inserts Viewer";

            ApplyTheming();
            SetupEventHandlers();
            LoadImagesFromDirectory();

            string currentTrackPath = GetCurrentTrackPath();
            if (!string.IsNullOrEmpty(currentTrackPath))
            {
                this.Text = $"Album Inserts Viewer - {Path.GetFileName(currentTrackPath)}";
            }

            if (!playing)
            {
                if (images == null || images.Length == 0)
                {
                    ShowNoImagesMessage();
                    return;
                }

                if (images.Length > 1 && config.AutoStartSlideshow)
                {
                    timer1.Start();
                    playing = true;
                }
            }
        }

        private void SetupEventHandlers()
        {
            // Timer
            timer1.Interval = config.SlideshowIntervalSeconds * 1000;
            timer1.Tick += Timer1_Tick;

            // Navigation buttons
            btnScans.Click += (s, e) => SwitchView(ViewMode.Scans);
            btnBooklet.Click += (s, e) => SwitchView(ViewMode.Booklet);

            // Picture box
            pictureBox1.Click += PictureBox1_Click;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // PDF button
            btnLaunchPdf.Click += LaunchPdfButton_Click;

            // Booklet panel resize
            bookletPanel.Resize += (sender, e) =>
            {
                btnLaunchPdf.Width = bookletPanel.Width - 40;
            };

            // Form dragging
            this.MouseDown += Form1_MouseDown;
            pictureBox1.MouseDown += Form1_MouseDown;
            txtNoImages.MouseDown += Form1_MouseDown;
            navPanel.MouseDown += Form1_MouseDown;
        }

        private void ApplyTheming()
        {
            this.BackColor = config.BackgroundColor;
            this.ForeColor = config.ForegroundColor;

            navPanel.BackColor = config.BackgroundColor;

            btnScans.BackColor = config.ButtonBackColor;
            btnScans.ForeColor = config.ButtonForeColor;
            btnBooklet.BackColor = config.ButtonBackColor;
            btnBooklet.ForeColor = config.ButtonForeColor;

            contentPanel.BackColor = config.BackgroundColor;
            scansPanel.BackColor = config.PanelBackColor;
            bookletPanel.BackColor = config.PanelBackColor;

            pictureBox1.BackColor = config.PanelBackColor;
            txtNoImages.BackColor = config.PanelBackColor;
            txtNoImages.ForeColor = config.ForegroundColor;

            lblPdfMessage.BackColor = config.PanelBackColor;
            lblPdfMessage.ForeColor = config.ForegroundColor;
            btnLaunchPdf.BackColor = config.ButtonBackColor;
            btnLaunchPdf.ForeColor = config.ButtonForeColor;

            SwitchView(ViewMode.Scans);
        }

        #endregion

        #region View Management

        private void SwitchView(ViewMode view)
        {
            currentView = view;

            switch (view)
            {
                case ViewMode.Scans:
                    scansPanel.Visible = true;
                    bookletPanel.Visible = false;
                    break;
                case ViewMode.Booklet:
                    scansPanel.Visible = false;
                    bookletPanel.Visible = true;
                    UpdatePdfTabUI();
                    break;
            }

            UpdateButtonAppearance();
        }

        private void UpdateButtonAppearance()
        {
            if (currentView == ViewMode.Scans)
            {
                btnScans.BackColor = config.ActiveButtonBackColor;
                btnScans.ForeColor = config.ActiveButtonForeColor;
                btnBooklet.BackColor = config.ButtonBackColor;
                btnBooklet.ForeColor = config.ButtonForeColor;
            }
            else
            {
                btnScans.BackColor = config.ButtonBackColor;
                btnScans.ForeColor = config.ButtonForeColor;
                btnBooklet.BackColor = config.ActiveButtonBackColor;
                btnBooklet.ForeColor = config.ActiveButtonForeColor;
            }
        }

        #endregion

        #region Form Dragging

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        #endregion

        #region UI State Management

        private void ShowNoImagesMessage()
        {
            pictureBox1.Visible = false;
            txtNoImages.Visible = true;
            txtNoImages.Text = "No images found\r\n\r\nSelect an album with image files or embedded artwork to display content.";

            hasPdfInCollection = false;
            currentPdfPath = null;

            timer1.Stop();
            playing = false;
        }

        private void ShowPictureBox()
        {
            txtNoImages.Visible = false;
            pictureBox1.Visible = true;
        }

        private void UpdatePdfTabUI()
        {
            if (hasPdfInCollection && !string.IsNullOrEmpty(currentPdfPath))
            {
                lblPdfMessage.Visible = true;
                btnLaunchPdf.Visible = true;
                string pdfName = Path.GetFileName(currentPdfPath);
                lblPdfMessage.Text = $"{pdfName}\r\n\r\nPDF booklet detected in album folder.\r\nUse the button below to launch the file externally.";
            }
            else
            {
                lblPdfMessage.Visible = true;
                btnLaunchPdf.Visible = false;
                lblPdfMessage.Text = "No PDF booklet detected.\r\n\r\nPDF booklets (liner notes, album inserts, etc.)\r\nwill appear here when available in the album folder.";
            }
        }

        #endregion

        #region Track and Directory Access

        private string GetCurrentTrackPath()
        {
            try
            {
                string trackPath = mbApi.NowPlaying_GetFileUrl();

                if (!string.IsNullOrEmpty(trackPath))
                {
                    if (trackPath.StartsWith("file:///"))
                    {
                        trackPath = trackPath.Substring(8);
                        trackPath = Uri.UnescapeDataString(trackPath);
                    }
                    else if (trackPath.StartsWith("file://"))
                    {
                        trackPath = trackPath.Substring(7);
                        trackPath = Uri.UnescapeDataString(trackPath);
                    }
                }

                return trackPath;
            }
            catch
            {
                return null;
            }
        }

        private string GetCurrentTrackDirectory()
        {
            string currentTrack = GetCurrentTrackPath();
            if (!string.IsNullOrEmpty(currentTrack))
            {
                return Path.GetDirectoryName(currentTrack);
            }
            return null;
        }

        #endregion

        #region File Search Methods

        private List<string> SearchImagesInAllSubfolders(string baseDirectory)
        {
            List<string> imageFiles = new List<string>();
            string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.pdf" };

            try
            {
                foreach (string extension in extensions)
                {
                    imageFiles.AddRange(Directory.GetFiles(baseDirectory, extension, SearchOption.AllDirectories));
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception)
            {
            }

            return imageFiles;
        }

        #endregion

        #region Image Loading and Display

        private void LoadImagesFromDirectory()
        {
            try
            {
                string currentTrackDir = GetCurrentTrackDirectory();

                if (string.IsNullOrEmpty(currentTrackDir))
                {
                    LoadCurrentTrackArtwork();
                    return;
                }

                List<string> imageFiles = SearchImagesInAllSubfolders(currentTrackDir);
                imageFiles = imageFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                List<string> pdfFiles = imageFiles.Where(f => Path.GetExtension(f).ToLower() == ".pdf").ToList();
                List<string> imageOnlyFiles = imageFiles.Where(f => Path.GetExtension(f).ToLower() != ".pdf").ToList();

                hasPdfInCollection = pdfFiles.Count > 0;
                currentPdfPath = hasPdfInCollection ? pdfFiles.First() : null;

                if (imageOnlyFiles.Count == 0)
                {
                    LoadCurrentTrackArtwork();
                    return;
                }

                images = imageOnlyFiles.ToArray();

                if (images.Length > 0)
                {
                    if (images.Length > 1 && config.AutoStartSlideshow)
                    {
                        if (!playing)
                        {
                            timer1.Start();
                            playing = true;
                        }
                    }
                    else
                    {
                        if (playing)
                        {
                            timer1.Stop();
                            playing = false;
                        }
                    }

                    DisplayImage(images[0]);
                }
            }
            catch
            {
                LoadCurrentTrackArtwork();
            }
        }

        private void DisplayImage(string filePath)
        {
            try
            {
                ShowPictureBox();

                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                pictureBox1.Image = System.Drawing.Image.FromFile(filePath);
                currentImagePath = filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying file {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private void LoadCurrentTrackArtwork()
        {
            try
            {
                string artworkUrl = mbApi.NowPlaying_GetArtwork();

                if (!string.IsNullOrEmpty(artworkUrl))
                {
                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = System.Drawing.Image.FromFile(artworkUrl);

                    images = new string[] { artworkUrl };
                    ShowPictureBox();

                    if (playing)
                    {
                        timer1.Stop();
                        playing = false;
                    }

                    currentImagePath = artworkUrl;
                }
                else
                {
                    pictureBox1.Image = null;
                    images = new string[0];
                    ShowNoImagesMessage();
                }
            }
            catch
            {
                pictureBox1.Image = null;
                images = new string[0];
                ShowNoImagesMessage();
            }
        }

        #endregion

        #region Event Handlers

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (images != null && images.Length > 1)
            {
                counter++;
                if (counter >= images.Length)
                {
                    counter = 0;
                }
                DisplayImage(images[counter]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Designer event - kept for compatibility
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(currentImagePath) && File.Exists(currentImagePath))
                {
                    Process.Start(currentImagePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LaunchPdfButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPdfPath) && File.Exists(currentPdfPath))
            {
                try
                {
                    Process.Start(currentPdfPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching PDF: {ex.Message}", "PDF Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Public Methods

        public void RefreshImagesForCurrentTrack()
        {
            counter = 0;
            LoadImagesFromDirectory();
        }

        #endregion

        #region Cleanup

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer1?.Stop();
            pictureBox1?.Image?.Dispose();
            base.OnFormClosed(e);
        }

        #endregion
    }
}