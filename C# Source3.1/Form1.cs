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
using static System.Net.Mime.MediaTypeNames;
using static MusicBeePlugin.Plugin;
using System.Runtime.InteropServices;
using PdfiumViewer;   // pdfviewer

namespace MusicBeePlugin
{
    public partial class Form1 : Form
    {
        int counter = 0;
        string[] images;
        bool playing = false;
        private MusicBeeApiInterface mbApi; // API interface reference

        // Import functions from user32.dll for dragging(form)
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private TextBox noImagesTextBox;
        private PdfViewer pdfViewer;   // ✅ Booklet tab PDF viewer

        // Array of folder names to search for
        private string[] targetFolders = { "Scans", "Artwork", "Booklet", "Insert", "Inserts", "Images", "Album Art", "scans", "artwork", "booklet", "insert", "inserts", "images", "album art" };

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        // Constructor
        public Form1(MusicBeeApiInterface apiInterface)
        {
            mbApi = apiInterface; // Store the reference
            InitializeComponent();
            InitializeNoImagesTextBox();
            InitializePdfViewer();   // ✅ Initialize booklet PDF viewer

            // Load images from target folders or fallback to cover art
            LoadImagesFromDirectory();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            this.MouseDown += Form1_MouseDown;
            pictureBox1.MouseDown += Form1_MouseDown;
            noImagesTextBox.MouseDown += Form1_MouseDown;

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
                timer1.Start();
                playing = true;
            }
            else
            {
                playing = false;
                timer1.Stop();
            }
        }

        /// <summary>
        /// Initialize the TextBox for "No images" message
        /// </summary>
        private void InitializeNoImagesTextBox()
        {
            noImagesTextBox = new TextBox();
            noImagesTextBox.Name = "noImagesTextBox";
            noImagesTextBox.ReadOnly = true;
            noImagesTextBox.Multiline = true;
            noImagesTextBox.TextAlign = HorizontalAlignment.Center;
            noImagesTextBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            noImagesTextBox.BackColor = SystemColors.Control;
            noImagesTextBox.BorderStyle = BorderStyle.None;
            noImagesTextBox.Visible = false;

            noImagesTextBox.Location = pictureBox1.Location;
            noImagesTextBox.Size = pictureBox1.Size;
            noImagesTextBox.Anchor = pictureBox1.Anchor;

            this.Controls.Add(noImagesTextBox);
            noImagesTextBox.BringToFront();
        }

        /// <summary>
        /// Initialize PDF viewer in Booklet tab
        /// </summary>
        private void InitializePdfViewer()
        {
            pdfViewer = new PdfViewer();
            pdfViewer.Dock = DockStyle.Fill;
            pdfViewer.ShowToolbar = true;
            pdfViewer.ShowBookmarks = false;
            tabPage2.Controls.Add(pdfViewer); // assumes tabPage2 = Booklet
        }

        private void ShowNoImagesMessage()
        {
            pictureBox1.Visible = false;
            noImagesTextBox.Visible = true;
            noImagesTextBox.Text = "No images found\r\n\r\nSelect an album with image files or embedded artwork to display content.";
            timer1.Stop();
            playing = false;
        }

        private void ShowPictureBox()
        {
            noImagesTextBox.Visible = false;
            pictureBox1.Visible = true;
        }

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

        private List<string> SearchAllImagesInDirectory(string directory)
        {
            List<string> imageFiles = new List<string>();
            string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.pdf" };

            if (!Directory.Exists(directory))
                return imageFiles;

            foreach (string extension in extensions)
            {
                imageFiles.AddRange(Directory.GetFiles(directory, extension, SearchOption.TopDirectoryOnly));
            }

            return imageFiles;
        }

        private Dictionary<string, string> GetCurrentTrackInfo()
        {
            var trackInfo = new Dictionary<string, string>();
            try
            {
                trackInfo["FilePath"] = mbApi.NowPlaying_GetFileUrl() ?? "";
                trackInfo["Artist"] = mbApi.NowPlaying_GetFileTag(MetaDataType.Artist) ?? "";
                trackInfo["Album"] = mbApi.NowPlaying_GetFileTag(MetaDataType.Album) ?? "";
                trackInfo["Title"] = mbApi.NowPlaying_GetFileTag(MetaDataType.TrackTitle) ?? "";
                trackInfo["Year"] = mbApi.NowPlaying_GetFileTag(MetaDataType.Year) ?? "";
                trackInfo["Genre"] = mbApi.NowPlaying_GetFileTag(MetaDataType.Genre) ?? "";
            }
            catch { }
            return trackInfo;
        }

        private List<string> SearchImagesInTargetFolders(string baseDirectory)
        {
            List<string> imageFiles = new List<string>();
            string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.pdf" };

            try
            {
                string[] subdirectories = Directory.GetDirectories(baseDirectory);
                foreach (string subdirectory in subdirectories)
                {
                    string folderName = Path.GetFileName(subdirectory);
                    if (targetFolders.Any(target => string.Equals(target, folderName, StringComparison.OrdinalIgnoreCase)))
                    {
                        foreach (string extension in extensions)
                        {
                            imageFiles.AddRange(Directory.GetFiles(subdirectory, extension, SearchOption.TopDirectoryOnly));
                        }
                    }
                }
            }
            catch { }

            return imageFiles;
        }

        private List<string> SearchCoverFiles(string directory)
        {
            List<string> coverFiles = new List<string>();
            string[] coverExtensions = { "jpg", "jpeg", "png", "bmp", "gif" };

            foreach (string extension in coverExtensions)
            {
                string coverPath = Path.Combine(directory, $"Cover.{extension}");
                if (File.Exists(coverPath))
                {
                    coverFiles.Add(coverPath);
                }
            }

            return coverFiles;
        }

        private void LoadImagesFromDirectory()
        {
            try
            {
                string currentTrackDir = GetCurrentTrackDirectory();

                if (string.IsNullOrEmpty(currentTrackDir))
                {
                    LoadCurrentTrackArtwork();
                    if (images == null || images.Length == 0)
                        ShowNoImagesMessage();
                    return;
                }

                List<string> imageFiles = SearchAllImagesInDirectory(currentTrackDir);
                List<string> targetFolderImages = SearchImagesInTargetFolders(currentTrackDir);
                imageFiles.AddRange(targetFolderImages);

                imageFiles = imageFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (imageFiles.Count == 0)
                {
                    imageFiles = SearchCoverFiles(currentTrackDir);
                }

                if (imageFiles.Count == 0)
                {
                    LoadCurrentTrackArtwork();
                    if (images == null || images.Length == 0)
                        ShowNoImagesMessage();
                    return;
                }

                images = imageFiles.ToArray();

                if (images.Length > 0)
                {
                    ShowPictureBox();
                    DisplayImage(images[0]);

                    if (images.Length > 1)
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
                }
                else
                {
                    ShowNoImagesMessage();
                }
            }
            catch
            {
                LoadCurrentTrackArtwork();
                if (images == null || images.Length == 0)
                    ShowNoImagesMessage();
            }
        }

        private void DisplayImage(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".pdf")
                {
                    LoadPdfInBooklet(filePath);
                    return;
                }

                pictureBox1.Image?.Dispose();
                pictureBox1.Image = System.Drawing.Image.FromFile(filePath);
                ShowPictureBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying file {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private void LoadPdfInBooklet(string filePath)
        {
            try
            {
                var document = PdfiumViewer.PdfDocument.Load(filePath);
                pdfViewer.Document?.Dispose();
                pdfViewer.Document = document;
                tabControl1.SelectedTab = tabPage2; // switch to booklet tab
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PDF: {ex.Message}");
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
                }
                else
                {
                    pictureBox1.Image = null;
                    images = new string[0];
                }
            }
            catch
            {
                pictureBox1.Image = null;
                images = new string[0];
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
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
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        public void RefreshImagesForCurrentTrack()
        {
            counter = 0;
            LoadImagesFromDirectory();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer1?.Stop();
            pictureBox1.Image?.Dispose();
            pdfViewer?.Document?.Dispose();
            base.OnFormClosed(e);
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
        }
    }
}
