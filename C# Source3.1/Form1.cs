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
        // Hardcoded directory path - change this to your desired directory
        string imageDirectory = @"E:\Github\AlbumInsertsViewer\TestCoverArts";
        string[] images;
        bool playing = false;
        private MusicBeeApiInterface mbApi; // API interface reference

        // Modified constructor to accept the API interface
        public Form1(MusicBeeApiInterface apiInterface)
        {
            InitializeComponent();
            mbApi = apiInterface; // Store the reference

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // Load images from current track directory or fallback to hardcoded directory
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
                    MessageBox.Show("No images available to display.");
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

        private void LoadImagesFromDirectory()
        {
            try
            {
                // Try to get current track directory first, fallback to hardcoded directory
                string currentTrackDir = GetCurrentTrackDirectory();
                string searchDirectory = !string.IsNullOrEmpty(currentTrackDir) ? currentTrackDir : imageDirectory;

                // Get all image files from the search directory
                string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.pdf" };
                List<string> imageFiles = new List<string>();

                foreach (string extension in extensions)
                {
                    imageFiles.AddRange(Directory.GetFiles(searchDirectory, extension, SearchOption.TopDirectoryOnly));
                }

                images = imageFiles.ToArray();

                // Display first image if any images found
                if (images.Length > 0)
                {
                    pictureBox1.Image = System.Drawing.Image.FromFile(images[0]);

                    // Update window title to show source directory
                    //this.Text += $" - Found {images.Length} images in: {Path.GetFileName(searchDirectory)}";
                }
                else
                {
                    string message = currentTrackDir != null
                        ? $"No images found in current track directory: {searchDirectory}\nTrying fallback directory: {imageDirectory}"
                        : $"No images found in directory: {searchDirectory}";

                    // If we tried current track dir and found nothing, try fallback
                    if (currentTrackDir != null && searchDirectory == currentTrackDir)
                    {
                        searchDirectory = imageDirectory;
                        foreach (string extension in extensions)
                        {
                            imageFiles.AddRange(Directory.GetFiles(searchDirectory, extension, SearchOption.TopDirectoryOnly));
                        }

                        images = imageFiles.ToArray();
                        if (images.Length > 0)
                        {
                            pictureBox1.Image = System.Drawing.Image.FromFile(images[0]);
                            //this.Text += $" - Found {images.Length} images in fallback directory";
                        }
                        else
                        {
                            MessageBox.Show($"No images found in either current track directory or fallback directory.");
                        }
                    }
                    else
                    {
                        MessageBox.Show(message);
                    }
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show($"Directory not found: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}");
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
                    //this.Text = "Album Inserts Viewer - Displaying track artwork";

                    // Create a single-item array for consistency with timer functionality
                    images = new string[] { artworkUrl };
                }
                else
                {
                    // No artwork available
                    pictureBox1.Image = null;
                    //this.Text = "Album Inserts Viewer - No artwork available";
                    images = new string[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading track artwork: {ex.Message}");
                pictureBox1.Image = null;
                //this.Text = "Album Inserts Viewer - No artwork available";
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
    }
}