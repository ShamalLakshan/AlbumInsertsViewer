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

namespace MusicBeePlugin
{
    public partial class Form1 : Form
    {
        int counter = 0;
        string[] images;
        bool playing = false;
        private MusicBeeApiInterface mbApi; // API interface reference

        // Add this TextBox - you'll need to add it to your form designer as well
        private TextBox noImagesTextBox;

        // Array of folder names to search for
        private string[] targetFolders = { "Scans", "Artwork", "Booklet", "Insert", "Inserts", "Images", "Album Art" };

        // Modified constructor to accept the API interface
        public Form1(MusicBeeApiInterface apiInterface)
        {
            InitializeComponent();
            mbApi = apiInterface; // Store the reference

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // Initialize the no images text box (you should also add this in the designer)
            InitializeNoImagesTextBox();

            // Load images from target folders or fallback to cover art
            LoadImagesFromDirectory();

            // Now you can get the current track path for debugging or other purposes
            string currentTrackPath = GetCurrentTrackPath();
            if (!string.IsNullOrEmpty(currentTrackPath))
            {
                // You can use this path for logging or display purposes
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
        /// Initialize the TextBox for displaying "No images found" message
        /// You should also add this TextBox in the Form Designer
        /// </summary>
        private void InitializeNoImagesTextBox()
        {
            // Create TextBox programmatically (alternatively, add it in the designer)
            noImagesTextBox = new TextBox();
            noImagesTextBox.Name = "noImagesTextBox";
            noImagesTextBox.ReadOnly = true;
            noImagesTextBox.Multiline = true;
            noImagesTextBox.TextAlign = HorizontalAlignment.Center;
            noImagesTextBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            noImagesTextBox.BackColor = SystemColors.Control;
            noImagesTextBox.BorderStyle = BorderStyle.None;
            noImagesTextBox.Visible = false; // Initially hidden

            // Position it over the PictureBox (adjust coordinates as needed)
            noImagesTextBox.Location = pictureBox1.Location;
            noImagesTextBox.Size = pictureBox1.Size;
            noImagesTextBox.Anchor = pictureBox1.Anchor;

            // Add to the form
            this.Controls.Add(noImagesTextBox);
            noImagesTextBox.BringToFront();
        }

        /// <summary>
        /// Show the "No images found" message and hide PictureBox
        /// </summary>
        private void ShowNoImagesMessage()
        {
            pictureBox1.Visible = false;
            noImagesTextBox.Visible = true;
            noImagesTextBox.Text = "No images found\r\n\r\nSelect an album with image files or embedded artwork to display content.";
            timer1.Stop();
            playing = false;
        }

        /// <summary>
        /// Show the PictureBox and hide the no images message
        /// </summary>
        private void ShowPictureBox()
        {
            noImagesTextBox.Visible = false;
            pictureBox1.Visible = true;
        }

        /// <summary>
        /// Gets the full file path of the currently playing track
        /// </summary>
        /// <returns>Full file path of current track, or null if error</returns>
        private string GetCurrentTrackPath()
        {
            try
            {
                return mbApi.NowPlaying_GetFileUrl();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting current track: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the directory containing the currently playing track
        /// </summary>
        /// <returns>Directory path of current track, or null if error</returns>
        private string GetCurrentTrackDirectory()
        {
            string currentTrack = GetCurrentTrackPath();
            if (!string.IsNullOrEmpty(currentTrack))
            {
                return System.IO.Path.GetDirectoryName(currentTrack);
            }
            return null;
        }

        /// <summary>
        /// Gets additional track information using the MusicBee API
        /// </summary>
        /// <returns>Dictionary with track metadata</returns>
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting track info: {ex.Message}");
            }

            return trackInfo;
        }

        /// <summary>
        /// Search for images in target folders within the current track directory
        /// </summary>
        /// <param name="baseDirectory">The directory to search in</param>
        /// <returns>List of image file paths found in target folders</returns>
        private List<string> SearchImagesInTargetFolders(string baseDirectory)
        {
            List<string> imageFiles = new List<string>();
            string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.pdf" };

            try
            {
                // Get all subdirectories in the base directory
                string[] subdirectories = Directory.GetDirectories(baseDirectory);

                foreach (string subdirectory in subdirectories)
                {
                    string folderName = Path.GetFileName(subdirectory);

                    // Check if the folder name matches any of our target folders (case-insensitive)
                    if (targetFolders.Any(target => string.Equals(target, folderName, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Search for images in this folder
                        foreach (string extension in extensions)
                        {
                            imageFiles.AddRange(Directory.GetFiles(subdirectory, extension, SearchOption.TopDirectoryOnly));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show message box
                Console.WriteLine($"Error searching target folders: {ex.Message}");
            }

            return imageFiles;
        }

        /// <summary>
        /// Search for cover files (Cover.jpg, Cover.png, etc.) in the current directory
        /// </summary>
        /// <param name="directory">Directory to search in</param>
        /// <returns>List of cover files found</returns>
        private List<string> SearchCoverFiles(string directory)
        {
            List<string> coverFiles = new List<string>();
            string[] coverExtensions = { "jpg", "jpeg", "png", "bmp", "gif" };

            try
            {
                foreach (string extension in coverExtensions)
                {
                    string coverPath = Path.Combine(directory, $"Cover.{extension}");
                    if (File.Exists(coverPath))
                    {
                        coverFiles.Add(coverPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching cover files: {ex.Message}");
            }

            return coverFiles;
        }

        private void LoadImagesFromDirectory()
        {
            try
            {
                // Try to get current track directory
                string currentTrackDir = GetCurrentTrackDirectory();

                if (string.IsNullOrEmpty(currentTrackDir))
                {
                    // No current track, try to load embedded artwork
                    LoadCurrentTrackArtwork();
                    if (images == null || images.Length == 0)
                    {
                        ShowNoImagesMessage();
                    }
                    return;
                }

                // First, search for images in target folders
                List<string> imageFiles = SearchImagesInTargetFolders(currentTrackDir);

                // If no images found in target folders, look for Cover.jpg/png files
                if (imageFiles.Count == 0)
                {
                    imageFiles = SearchCoverFiles(currentTrackDir);
                }

                // If still no images found, try embedded artwork
                if (imageFiles.Count == 0)
                {
                    LoadCurrentTrackArtwork();
                    if (images == null || images.Length == 0)
                    {
                        ShowNoImagesMessage();
                    }
                    return;
                }

                // Convert to array and display
                images = imageFiles.ToArray();

                if (images.Length > 0)
                {
                    ShowPictureBox();
                    DisplayImage(images[0]);

                    // Start timer only if we have multiple images to cycle through
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
                        // Only one image, no need to cycle
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
            catch (DirectoryNotFoundException)
            {
                // Try embedded artwork as fallback
                LoadCurrentTrackArtwork();
                if (images == null || images.Length == 0)
                {
                    ShowNoImagesMessage();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied: {ex.Message}");
                ShowNoImagesMessage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}");
                ShowNoImagesMessage();
            }
        }

        /// <summary>
        /// Load and display an image or PDF file
        /// </summary>
        /// <param name="filePath">Path to the image or PDF file</param>
        private void DisplayImage(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".pdf")
                {
                    // For PDF files, you might want to show a message or handle differently
                    // For now, just show a placeholder or the first page (requires additional PDF library)
                    MessageBox.Show($"PDF file detected: {Path.GetFileName(filePath)}\nPDF viewing not implemented yet.");
                    return;
                }

                // For image files
                pictureBox1.Image?.Dispose(); // Dispose previous image
                pictureBox1.Image = System.Drawing.Image.FromFile(filePath);
                ShowPictureBox(); // Ensure PictureBox is visible
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying file {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        /// <summary>
        /// Load current track artwork from MusicBee as fallback
        /// </summary>
        private void LoadCurrentTrackArtwork()
        {
            try
            {
                string artworkUrl = mbApi.NowPlaying_GetArtwork();

                if (!string.IsNullOrEmpty(artworkUrl))
                {
                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = System.Drawing.Image.FromFile(artworkUrl);

                    // Create a single-item array for consistency with timer functionality
                    images = new string[] { artworkUrl };
                    ShowPictureBox();

                    // Stop timer since we only have one image
                    if (playing)
                    {
                        timer1.Stop();
                        playing = false;
                    }
                }
                else
                {
                    // No artwork available
                    pictureBox1.Image = null;
                    images = new string[0];
                }
            }
            catch (Exception ex)
            {
                // Don't show error for missing artwork, just set empty
                pictureBox1.Image = null;
                images = new string[0];
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (images != null && images.Length > 1) // Only cycle if more than 1 file
            {
                counter++;
                if (counter >= images.Length)
                {
                    counter = 0;
                }

                DisplayImage(images[counter]);
                //filename.Text = Path.GetFileName(images[counter]); //show name of the image if you have a label
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // You can add additional initialization here if needed
            // For example, you could display current track info in a label
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //// You could add functionality here, like showing track info on click
            //var trackInfo = GetCurrentTrackInfo();
            //string info = $"Current Track Info:\n" +
            //             $"Artist: {trackInfo["Artist"]}\n" +
            //             $"Album: {trackInfo["Album"]}\n" +
            //             $"Title: {trackInfo["Title"]}\n" +
            //             $"Year: {trackInfo["Year"]}\n" +
            //             $"Genre: {trackInfo["Genre"]}\n" +
            //             $"File: {trackInfo["FilePath"]}";

            //MessageBox.Show(info, "Track Information");
        }

        /// <summary>
        /// Public method to refresh images when track changes
        /// Can be called from the plugin when receiving track change notifications
        /// </summary>
        public void RefreshImagesForCurrentTrack()
        {
            counter = 0; // Reset counter
            LoadImagesFromDirectory();
        }

        /// <summary>
        /// Clean up resources when form is closing
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer1?.Stop();
            pictureBox1.Image?.Dispose();
            base.OnFormClosed(e);
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}   