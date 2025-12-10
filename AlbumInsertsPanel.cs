using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class AlbumInsertsPanel : UserControl
    {
        #region Fields

        private int counter = 0;
        private string[] images;
        private bool playing = false;
        private MusicBeeApiInterface mbApi;
        private string currentImagePath;
        private string currentPdfPath;
        private bool hasPdfInCollection = false;

        private TabControl tabControl;
        private TabPage imagesTab;
        private TabPage bookletTab;
        private PictureBox pictureBox;
        private Timer timer;
        private TextBox noImagesTextBox;
        private Label pdfMessageLabel;
        private Button launchPdfButton;
        private Label openImageLabel;

        #endregion

        #region Constructor and Initialization

        public AlbumInsertsPanel(MusicBeeApiInterface apiInterface)
        {
            mbApi = apiInterface;
            InitializeComponent();
            LoadImagesFromDirectory();

            if (images != null && images.Length > 1)
            {
                timer.Start();
                playing = true;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            imagesTab = new TabPage("Scans");

            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Click += PictureBox_Click;
            imagesTab.Controls.Add(pictureBox);

            noImagesTextBox = new TextBox();
            noImagesTextBox.Dock = DockStyle.Fill;
            noImagesTextBox.ReadOnly = true;
            noImagesTextBox.Multiline = true;
            noImagesTextBox.TextAlign = HorizontalAlignment.Center;
            noImagesTextBox.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
            noImagesTextBox.BorderStyle = BorderStyle.None;
            noImagesTextBox.Visible = false;
            imagesTab.Controls.Add(noImagesTextBox);
            noImagesTextBox.BringToFront();

            openImageLabel = new Label();
            openImageLabel.Text = "🔗 Open in viewer";
            openImageLabel.AutoSize = true;
            openImageLabel.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Underline);
            openImageLabel.Cursor = Cursors.Hand;
            openImageLabel.BackColor = Color.Transparent;
            openImageLabel.Visible = false;
            openImageLabel.Click += OpenImageLabel_Click;
            imagesTab.Controls.Add(openImageLabel);
            openImageLabel.BringToFront();

            Action positionOpenImageLabel = () =>
            {
                openImageLabel.Left = pictureBox.Right - openImageLabel.Width - 10;
                openImageLabel.Top = pictureBox.Bottom - openImageLabel.Height - 10;
            };

            this.Resize += (sender, e) => positionOpenImageLabel();
            openImageLabel.TextChanged += (sender, e) => positionOpenImageLabel();

            openImageLabel.MouseEnter += (sender, e) =>
            {
                Color currentColor = openImageLabel.ForeColor;
                int brighten = 60;
                openImageLabel.ForeColor = Color.FromArgb(
                    Math.Min(255, currentColor.R + brighten),
                    Math.Min(255, currentColor.G + brighten),
                    Math.Min(255, currentColor.B + brighten)
                );
            };
            openImageLabel.MouseLeave += (sender, e) =>
            {
                openImageLabel.ForeColor = this.ForeColor;
            };

            bookletTab = new TabPage("Booklet");

            pdfMessageLabel = new Label();
            pdfMessageLabel.Dock = DockStyle.Top;
            pdfMessageLabel.Height = 100;
            pdfMessageLabel.TextAlign = ContentAlignment.MiddleCenter;
            pdfMessageLabel.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
            pdfMessageLabel.Visible = false;
            bookletTab.Controls.Add(pdfMessageLabel);

            launchPdfButton = new Button();
            launchPdfButton.Text = "Launch in External Viewer";
            launchPdfButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            launchPdfButton.Height = 35;
            launchPdfButton.Width = bookletTab.Width - 40;
            launchPdfButton.Left = 20;
            launchPdfButton.Top = 120;
            launchPdfButton.Click += LaunchPdfButton_Click;
            launchPdfButton.Visible = false;
            launchPdfButton.FlatStyle = FlatStyle.Flat;
            bookletTab.Controls.Add(launchPdfButton);

            bookletTab.Resize += (sender, e) =>
            {
                launchPdfButton.Width = bookletTab.Width - 40;
            };

            tabControl.TabPages.Add(imagesTab);
            tabControl.TabPages.Add(bookletTab);

            timer = new Timer();
            timer.Interval = 3000;
            timer.Tick += Timer_Tick;

            this.Controls.Add(tabControl);

            this.ResumeLayout(false);
        }

        #endregion

        #region UI State Management

        private void ShowNoImagesMessage()
        {
            pictureBox.Visible = false;
            noImagesTextBox.Visible = true;
            noImagesTextBox.Text = "No images found\r\n\r\nSelect an album with image files or embedded artwork to display content.";

            pdfMessageLabel.Visible = false;
            launchPdfButton.Visible = false;
            openImageLabel.Visible = false;
            hasPdfInCollection = false;
            currentPdfPath = null;

            timer.Stop();
            playing = false;
        }

        private void ShowPictureBox()
        {
            noImagesTextBox.Visible = false;
            pictureBox.Visible = true;
            openImageLabel.Visible = true;
        }

        private void UpdatePdfTabUI()
        {
            if (hasPdfInCollection && !string.IsNullOrEmpty(currentPdfPath))
            {
                pdfMessageLabel.Visible = true;
                launchPdfButton.Visible = true;
                string pdfName = Path.GetFileName(currentPdfPath);
                pdfMessageLabel.Text = $"📄 {pdfName}\r\n\r\nPDF booklet detected in album folder.\r\nUse the button below to launch the file externally.";
            }
            else
            {
                pdfMessageLabel.Visible = true;
                launchPdfButton.Visible = false;
                pdfMessageLabel.Text = "No PDF booklet detected.\r\n\r\nPDF booklets (liner notes, album inserts, etc.)\r\nwill appear here when available in the album folder.";
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
                    if (images.Length > 1)
                    {
                        if (!playing)
                        {
                            timer.Start();
                            playing = true;
                        }
                    }
                    else
                    {
                        if (playing)
                        {
                            timer.Stop();
                            playing = false;
                        }
                    }

                    UpdatePdfTabUI();
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

                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                }

                pictureBox.Image = Image.FromFile(filePath);
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
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = Image.FromFile(artworkUrl);

                    images = new string[] { artworkUrl };
                    ShowPictureBox();

                    if (playing)
                    {
                        timer.Stop();
                        playing = false;
                    }

                    UpdatePdfTabUI();

                    openImageLabel.Visible = true;
                    currentImagePath = artworkUrl;
                }
                else
                {
                    pictureBox.Image = null;
                    images = new string[0];
                    ShowNoImagesMessage();
                }
            }
            catch
            {
                pictureBox.Image = null;
                images = new string[0];
                ShowNoImagesMessage();
            }
        }

        #endregion

        #region Event Handlers

        private void Timer_Tick(object sender, EventArgs e)
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

        private void PictureBox_Click(object sender, EventArgs e)
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

        private void OpenImageLabel_Click(object sender, EventArgs e)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Stop();
                timer?.Dispose();
                pictureBox?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}