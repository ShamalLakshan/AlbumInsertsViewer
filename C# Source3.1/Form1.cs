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

namespace MusicBeePlugin
{
    public partial class Form1 : Form
    {
        int counter = 0;
        // Hardcoded directory path - change this to your desired directory
        string imageDirectory = @"E:\Github\AlbumInsertsViewer\TestCoverArts";
        string[] images;
        bool playing = false;

        public Form1()
        {
            InitializeComponent();

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // Load images from hardcoded directory at startup
            LoadImagesFromDirectory();

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

        private void LoadImagesFromDirectory()
        {
            try
            {
                // Get all image files from the hardcoded directory
                string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" };
                List<string> imageFiles = new List<string>();

                foreach (string extension in extensions)
                {
                    imageFiles.AddRange(Directory.GetFiles(imageDirectory, extension, SearchOption.TopDirectoryOnly));
                }

                images = imageFiles.ToArray();

                // Display first image if any images found
                if (images.Length > 0)
                {
                    pictureBox1.Image = System.Drawing.Image.FromFile(images[0]);
                }
                else
                {
                    MessageBox.Show($"No images found in directory: {imageDirectory}");
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show($"Directory not found: {imageDirectory}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (images != null && images.Length > 0)
            {
                counter++;
                if (counter >= images.Length)
                {
                    counter = 0;
                }

                try
                {
                    pictureBox1.Image?.Dispose(); // Dispose previous image to prevent memory leaks
                    pictureBox1.Image = System.Drawing.Image.FromFile(images[counter]);
                    //filename.Text = Path.GetFileName(images[counter]); //show name of the image
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}");
                }
            }
        }
    }
}