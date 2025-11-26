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
using System.Diagnostics;

namespace MusicBeePlugin
{
    /**
     * @class Form1
     * @brief Main form for the Album Inserts Viewer plugin
     * 
     * This form provides a dual-tab interface for viewing album artwork and PDF booklets.
     * The Images tab displays a slideshow of album artwork found in the track's directory,
     * while the Booklet tab provides access to PDF liner notes and inserts.
     * 
     * Features:
     * - Automatic slideshow of multiple images
     * - Recursive directory search for images and PDFs
     * - Fallback to embedded album artwork
     * - External viewer launch for images and PDFs
     * - Draggable borderless window
     */
    public partial class Form1 : Form
    {
        #region Fields

        /**
         * @brief Current index in the images array for slideshow cycling
         */
        private int counter = 0;

        /**
         * @brief Array of image file paths to display in slideshow
         */
        private string[] images;

        /**
         * @brief Indicates whether the slideshow timer is currently running
         */
        private bool playing = false;

        /**
         * @brief Reference to MusicBee's API interface for accessing track information
         */
        private MusicBeeApiInterface mbApi;

        /**
         * @brief Path to the currently displayed image file
         */
        private string currentImagePath;

        /**
         * @brief Path to the detected PDF booklet file
         */
        private string currentPdfPath;

        /**
         * @brief Flag indicating if a PDF booklet was found in the album folder
         */
        private bool hasPdfInCollection = false;

        /**
         * @brief TextBox control displaying "No images found" message when no content is available
         */
        private TextBox noImagesTextBox;

        /**
         * @brief Label control for displaying PDF booklet status on the Booklet tab
         */
        private Label pdfMessageLabel;

        /**
         * @brief Button to launch PDF booklet in external viewer
         */
        private Button launchPdfButton;

        /**
         * @brief Subtle clickable label to open current image in external viewer
         */
        private Label openImageLabel;

        #endregion

        #region Windows API Imports for Form Dragging

        /**
         * @brief Releases mouse capture from the current window
         * @return True if successful, false otherwise
         */
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        /**
         * @brief Sends a message to a window
         * @param hWnd Handle to the window
         * @param Msg Message to send
         * @param wParam Additional message-specific information
         * @param lParam Additional message-specific information
         * @return The result of the message processing
         */
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        /** @brief Window message for non-client area left button down */
        private const int WM_NCLBUTTONDOWN = 0xA1;
        
        /** @brief Hit test code for window caption/title bar */
        private const int HTCAPTION = 0x2;

        #endregion

        #region Constructor and Initialization

        /**
         * @brief Initializes the form with MusicBee API interface and sets up all UI components
         * @param apiInterface MusicBee API interface for accessing player data
         * 
         * This constructor performs the following initialization:
         * 1. Sets up the PDF viewer UI on the Booklet tab
         * 2. Creates the "Open in viewer" link for images
         * 3. Configures the image display settings
         * 4. Enables form dragging functionality
         * 5. Loads initial image content from the current track
         */
        public Form1(MusicBeeApiInterface apiInterface)
        {
            mbApi = apiInterface;
            InitializeComponent();
            InitializeNoImagesTextBox();

            // ===== PDF VIEWER UI SETUP (tabPage2 - Booklet Tab) =====
            pdfMessageLabel = new Label();
            pdfMessageLabel.Dock = DockStyle.Top;
            pdfMessageLabel.Height = 100;
            pdfMessageLabel.TextAlign = ContentAlignment.MiddleCenter;
            pdfMessageLabel.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
            pdfMessageLabel.Visible = false;
            tabPage2.Controls.Add(pdfMessageLabel);

            launchPdfButton = new Button();
            launchPdfButton.Text = "Launch in External Viewer";
            launchPdfButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            launchPdfButton.Height = 35;
            launchPdfButton.Width = tabPage2.Width - 40;
            launchPdfButton.Left = 20;
            launchPdfButton.Top = 120;
            launchPdfButton.Click += LaunchPdfButton_Click;
            launchPdfButton.Visible = false;

            // Update button width when form is resized
            tabPage2.Resize += (sender, e) =>
            {
                launchPdfButton.Width = tabPage2.Width - 40;
            };

            tabPage2.Controls.Add(launchPdfButton);

            // ===== OPEN IMAGE LABEL SETUP (tabPage1 - Images Tab) =====
            // Small clickable link in bottom-right corner to open current image externally
            openImageLabel = new Label();
            openImageLabel.Text = "🔗 Open in viewer";
            openImageLabel.AutoSize = true;
            openImageLabel.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Underline);
            openImageLabel.ForeColor = Color.FromArgb(100, 100, 100); // Subtle gray
            openImageLabel.Cursor = Cursors.Hand;
            openImageLabel.BackColor = Color.Transparent;
            openImageLabel.Visible = false;
            openImageLabel.Click += OpenImageLabel_Click;

            // Position label in bottom-right corner of the image display area
            Action positionOpenImageLabel = () =>
            {
                openImageLabel.Left = pictureBox1.Right - openImageLabel.Width - 10;
                openImageLabel.Top = pictureBox1.Bottom - openImageLabel.Height - 10;
            };

            tabPage1.Controls.Add(openImageLabel);
            openImageLabel.BringToFront();

            // Update position when form resizes or text changes
            this.Resize += (sender, e) => positionOpenImageLabel();
            openImageLabel.TextChanged += (sender, e) => positionOpenImageLabel();

            // Hover effect - darken text when mouse hovers over
            openImageLabel.MouseEnter += (sender, e) =>
            {
                openImageLabel.ForeColor = Color.FromArgb(50, 50, 50);
            };
            openImageLabel.MouseLeave += (sender, e) =>
            {
                openImageLabel.ForeColor = Color.FromArgb(100, 100, 100);
            };

            // ===== LOAD IMAGES AND CONFIGURE DISPLAY =====
            LoadImagesFromDirectory();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom; // Maintain aspect ratio

            // Enable form dragging by clicking on various UI elements
            this.MouseDown += Form1_MouseDown;
            pictureBox1.MouseDown += Form1_MouseDown;
            noImagesTextBox.MouseDown += Form1_MouseDown;

            // Update form title with current track name
            string currentTrackPath = GetCurrentTrackPath();
            if (!string.IsNullOrEmpty(currentTrackPath))
            {
                this.Text = $"Album Inserts Viewer - {Path.GetFileName(currentTrackPath)}";
            }

            // Start slideshow timer if multiple images are available
            if (!playing)
            {
                if (images == null || images.Length == 0)
                {
                    ShowNoImagesMessage();
                    return;
                }

                if (images.Length > 1)
                {
                    timer1.Start();
                    playing = true;
                }
            }
        }

        /**
         * @brief Initializes the TextBox that displays "No images found" message
         * 
         * Creates and configures a TextBox control that appears when no images or
         * embedded artwork is available for the current track. The TextBox matches
         * the position and size of the main PictureBox.
         */
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

            // Match the position and size of the PictureBox
            noImagesTextBox.Location = pictureBox1.Location;
            noImagesTextBox.Size = pictureBox1.Size;
            noImagesTextBox.Anchor = pictureBox1.Anchor;

            this.Controls.Add(noImagesTextBox);
            noImagesTextBox.BringToFront();
        }

        #endregion

        #region Form Dragging

        /**
         * @brief Enables dragging the form by clicking and holding on any part of it
         * @param sender The control that triggered the event
         * @param e Mouse event arguments
         * 
         * Uses Windows API to allow form dragging from any clickable area,
         * making the borderless form movable by the user.
         */
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

        /**
         * @brief Displays "No images found" message when no content is available
         * 
         * Hides the image display and shows an informative message to the user.
         * Also stops the slideshow timer and resets all PDF-related state variables.
         */
        private void ShowNoImagesMessage()
        {
            pictureBox1.Visible = false;
            noImagesTextBox.Visible = true;
            noImagesTextBox.Text = "No images found\r\n\r\nSelect an album with image files or embedded artwork to display content.";

            pdfMessageLabel.Visible = false;
            launchPdfButton.Visible = false;
            openImageLabel.Visible = false;
            hasPdfInCollection = false;
            currentPdfPath = null;

            timer1.Stop();
            playing = false;
        }

        /**
         * @brief Shows the PictureBox and hides the "No images" message
         * 
         * Switches the display back to image viewing mode and shows the
         * "Open in viewer" link for external viewing access.
         */
        private void ShowPictureBox()
        {
            noImagesTextBox.Visible = false;
            pictureBox1.Visible = true;
            openImageLabel.Visible = true;
        }

        /**
         * @brief Updates the PDF booklet tab UI based on whether a PDF was detected
         * 
         * If a PDF is found in the album folder:
         * - Displays the PDF filename with an icon
         * - Shows the launch button for external viewing
         * 
         * If no PDF is found:
         * - Displays an informative message explaining what would appear
         * - Hides the launch button
         */
        private void UpdatePdfTabUI()
        {
            if (hasPdfInCollection && !string.IsNullOrEmpty(currentPdfPath))
            {
                // PDF found - show file name and launch button
                pdfMessageLabel.Visible = true;
                launchPdfButton.Visible = true;
                string pdfName = Path.GetFileName(currentPdfPath);
                pdfMessageLabel.Text = $"📄 {pdfName}\r\n\r\nPDF booklet detected in album folder.\r\nUse the button below to launch the file externally.";
            }
            else
            {
                // No PDF - show informative message
                pdfMessageLabel.Visible = true;
                launchPdfButton.Visible = false;
                pdfMessageLabel.Text = "No PDF booklet detected.\r\n\r\nPDF booklets (liner notes, album inserts, etc.)\r\nwill appear here when available in the album folder.";
            }
        }

        #endregion

        #region Track and Directory Access

        /**
         * @brief Gets the file path of the currently playing track from MusicBee
         * @return Full file path of the current track, or null if unavailable
         * 
         * Retrieves the track path from MusicBee API and handles URL decoding
         * by removing file:// protocol prefixes and unescaping special characters.
         * This is necessary because MusicBee returns URLs in file:// format.
         */
        private string GetCurrentTrackPath()
        {
            try
            {
                string trackPath = mbApi.NowPlaying_GetFileUrl();

                if (!string.IsNullOrEmpty(trackPath))
                {
                    // Remove file:// or file:/// protocol prefix and decode URL encoding
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

        /**
         * @brief Gets the directory containing the currently playing track
         * @return Directory path, or null if unavailable
         * 
         * Extracts the directory path from the current track's file path.
         * This directory is used as the root for searching album artwork and PDFs.
         */
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

        /**
         * @brief Recursively searches for all image and PDF files in the specified directory and its subdirectories
         * @param baseDirectory Root directory to start searching from
         * @return List of file paths for all found images and PDFs
         * 
         * Searches for the following file types:
         * - Images: JPG, JPEG, PNG, BMP, GIF
         * - Documents: PDF
         * 
         * The search is performed recursively through all subdirectories.
         * Handles permission errors gracefully by skipping inaccessible folders.
         */
        private List<string> SearchImagesInAllSubfolders(string baseDirectory)
        {
            List<string> imageFiles = new List<string>();
            string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.pdf" };

            try
            {
                // Search recursively through all subdirectories
                foreach (string extension in extensions)
                {
                    imageFiles.AddRange(Directory.GetFiles(baseDirectory, extension, SearchOption.AllDirectories));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip folders we don't have permission to access
            }
            catch (Exception)
            {
                // Handle other potential errors silently
            }

            return imageFiles;
        }

        #endregion

        #region Image Loading and Display

        /**
         * @brief Main method to load and prepare images from the current track's album folder
         * 
         * This method performs the following workflow:
         * 1. Gets the current track's directory
         * 2. Recursively searches for image and PDF files
         * 3. Separates images from PDFs (PDFs go to Booklet tab, images to slideshow)
         * 4. Removes duplicate files (case-insensitive comparison)
         * 5. Falls back to embedded artwork if no image files found
         * 6. Configures and starts the slideshow timer if multiple images exist
         * 7. Displays the first image in the collection
         */
        private void LoadImagesFromDirectory()
        {
            try
            {
                string currentTrackDir = GetCurrentTrackDirectory();

                // No track directory available - try embedded artwork
                if (string.IsNullOrEmpty(currentTrackDir))
                {
                    LoadCurrentTrackArtwork();
                    return;
                }

                // Search for all image and PDF files recursively
                List<string> imageFiles = SearchImagesInAllSubfolders(currentTrackDir);
                imageFiles = imageFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                // Separate PDFs from images - PDFs go to Booklet tab, images to slideshow
                List<string> pdfFiles = imageFiles.Where(f => Path.GetExtension(f).ToLower() == ".pdf").ToList();
                List<string> imageOnlyFiles = imageFiles.Where(f => Path.GetExtension(f).ToLower() != ".pdf").ToList();

                // Store PDF info for the Booklet tab
                hasPdfInCollection = pdfFiles.Count > 0;
                currentPdfPath = hasPdfInCollection ? pdfFiles.First() : null;

                // No image files found - fall back to embedded artwork
                if (imageOnlyFiles.Count == 0)
                {
                    LoadCurrentTrackArtwork();
                    return;
                }

                images = imageOnlyFiles.ToArray();

                if (images.Length > 0)
                {
                    // Start/stop timer based on number of images
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

                    // Update the Booklet tab UI
                    UpdatePdfTabUI();

                    // Display the first image
                    DisplayImage(images[0]);
                }
            }
            catch
            {
                // On any error, fall back to embedded artwork
                LoadCurrentTrackArtwork();
            }
        }

        /**
         * @brief Displays a single image file in the PictureBox
         * @param filePath Path to the image file to display
         * 
         * Loads and displays the image while properly disposing of the previous image
         * to prevent memory leaks. Uses PictureBoxSizeMode.Zoom to maintain aspect ratio.
         */
        private void DisplayImage(string filePath)
        {
            try
            {
                ShowPictureBox();

                // Dispose previous image to free memory
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

        /**
         * @brief Loads the embedded album artwork from MusicBee as a fallback when no image files are found
         * 
         * This method is called when:
         * - No track directory is available (e.g., streaming)
         * - No image files are found in the album folder
         * - An error occurs during file searching
         * 
         * If embedded artwork is available:
         * - Displays it in the PictureBox
         * - Stops the slideshow timer (only one image)
         * - Updates PDF tab state (may still have PDFs from folder scan)
         * 
         * If no embedded artwork is available:
         * - Displays "No images found" message
         */
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

                    // Stop slideshow timer since we only have one image
                    if (playing)
                    {
                        timer1.Stop();
                        playing = false;
                    }

                    // Update PDF tab (may still have PDFs from folder scan)
                    UpdatePdfTabUI();

                    openImageLabel.Visible = true;
                    currentImagePath = artworkUrl;
                }
                else
                {
                    // No artwork available at all
                    pictureBox1.Image = null;
                    images = new string[0];
                    ShowNoImagesMessage();
                }
            }
            catch
            {
                // Failed to load artwork
                pictureBox1.Image = null;
                images = new string[0];
                ShowNoImagesMessage();
            }
        }

        #endregion

        #region Event Handlers

        /**
         * @brief Timer tick event - cycles through images in the slideshow
         * @param sender Event sender (timer1)
         * @param e Event arguments
         * 
         * Automatically advances to the next image in the slideshow when the timer fires.
         * Loops back to the first image after reaching the end of the array.
         * Only executes if there are 2 or more images available.
         */
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

        /**
         * @brief Form load event (currently unused but kept for future extensibility)
         * @param sender Event sender
         * @param e Event arguments
         */
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        /**
         * @brief Handles click on the PictureBox - opens current image in default application
         * @param sender Event sender (pictureBox1)
         * @param e Event arguments
         * 
         * Opens the currently displayed image in the system's default image viewer.
         * This provides an alternative method to the "Open in viewer" label.
         */
        private void pictureBox1_Click(object sender, EventArgs e)
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

        /**
         * @brief Handles click on the "Open in viewer" label - launches current image in default application
         * @param sender Event sender (openImageLabel)
         * @param e Event arguments
         * 
         * Opens the currently displayed image in the system's default image viewer.
         * Provides a visible, clickable link for users who prefer explicit controls.
         */
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

        /**
         * @brief Handles click on the PDF launch button - opens PDF in default PDF viewer
         * @param sender Event sender (launchPdfButton)
         * @param e Event arguments
         * 
         * Launches the detected PDF booklet in the system's default PDF viewer application.
         * Common PDF viewers include Adobe Reader, Foxit, Edge, Chrome, etc.
         */
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

        /**
         * @brief Public method to refresh the image display when track changes
         * 
         * Called from the plugin's main class (Plugin.cs) when a new track starts playing.
         * Resets the slideshow counter and reloads images from the new track's folder.
         * This ensures the viewer always displays content relevant to the current track.
         */
        public void RefreshImagesForCurrentTrack()
        {
            counter = 0;
            LoadImagesFromDirectory();
        }

        #endregion

        #region Cleanup

        /**
         * @brief Cleanup when form is closed - stops timer and disposes image resources
         * @param e Form closed event arguments
         * 
         * Ensures proper resource cleanup to prevent memory leaks when the form is closed.
         * Stops the slideshow timer and disposes of the currently loaded image.
         */
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer1?.Stop();
            pictureBox1.Image?.Dispose();
            base.OnFormClosed(e);
        }

        #endregion
    }
}