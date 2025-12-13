using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();
        private Form1 albumInsertsForm;
        private AlbumInsertsPanel dockablePanel;
        private string configFilePath;
        private PluginConfig config;

        public class PluginConfig
        {
            // Color settings
            public Color BackgroundColor { get; set; }
            public Color ForegroundColor { get; set; }
            public Color ButtonBackColor { get; set; }
            public Color ButtonForeColor { get; set; }
            public Color ActiveButtonBackColor { get; set; }
            public Color ActiveButtonForeColor { get; set; }
            public Color PanelBackColor { get; set; }

            // Slideshow settings
            public int SlideshowIntervalSeconds { get; set; }
            public bool AutoStartSlideshow { get; set; }

            // Display settings
            public bool ShowOpenInViewerLink { get; set; }
            public int WindowWidth { get; set; }
            public int WindowHeight { get; set; }

            public PluginConfig()
            {
                // Default: Everything black with white text
                BackgroundColor = Color.Black;
                ForegroundColor = Color.White;
                ButtonBackColor = Color.FromArgb(30, 30, 30);
                ButtonForeColor = Color.White;
                ActiveButtonBackColor = Color.FromArgb(60, 60, 60);
                ActiveButtonForeColor = Color.White;
                PanelBackColor = Color.Black;

                // Default slideshow settings
                SlideshowIntervalSeconds = 3;
                AutoStartSlideshow = true;

                // Default display settings
                ShowOpenInViewerLink = true;
                WindowWidth = 800;
                WindowHeight = 600;
            }
        }

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "Album Inserts Viewer";
            about.Description = "A plugin to display the scans/booklets/artwork included inside an album.";
            about.Author = "Shamal Lakshan";
            about.TargetApplication = "AlbumInsertsViewer";
            about.Type = PluginType.General;
            about.VersionMajor = 1;
            about.VersionMinor = 0;
            about.Revision = 2;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;

            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            configFilePath = Path.Combine(dataPath, "albuminsertsviewer.colors.conf");

            LoadOrCreateConfig();
            createMenuItem();
            return about;
        }

        private void LoadOrCreateConfig()
        {
            config = new PluginConfig();

            if (!File.Exists(configFilePath))
            {
                CreateDefaultConfig();
            }
            else
            {
                LoadConfig();
            }
        }

        private void CreateDefaultConfig()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(configFilePath))
                {
                    writer.WriteLine("# Album Inserts Viewer Configuration");
                    writer.WriteLine("# Colors are in R,G,B format (0-255 for each component)");
                    writer.WriteLine("# Edit these values to match your MusicBee theme");
                    writer.WriteLine();
                    writer.WriteLine("# ===== COLOR SETTINGS =====");
                    writer.WriteLine();
                    writer.WriteLine("# Main background color");
                    writer.WriteLine($"BackgroundColor={config.BackgroundColor.R},{config.BackgroundColor.G},{config.BackgroundColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Main text/foreground color");
                    writer.WriteLine($"ForegroundColor={config.ForegroundColor.R},{config.ForegroundColor.G},{config.ForegroundColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Navigation button background (inactive)");
                    writer.WriteLine($"ButtonBackColor={config.ButtonBackColor.R},{config.ButtonBackColor.G},{config.ButtonBackColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Navigation button text (inactive)");
                    writer.WriteLine($"ButtonForeColor={config.ButtonForeColor.R},{config.ButtonForeColor.G},{config.ButtonForeColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Navigation button background (active/selected)");
                    writer.WriteLine($"ActiveButtonBackColor={config.ActiveButtonBackColor.R},{config.ActiveButtonBackColor.G},{config.ActiveButtonBackColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Navigation button text (active/selected)");
                    writer.WriteLine($"ActiveButtonForeColor={config.ActiveButtonForeColor.R},{config.ActiveButtonForeColor.G},{config.ActiveButtonForeColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Content panel background");
                    writer.WriteLine($"PanelBackColor={config.PanelBackColor.R},{config.PanelBackColor.G},{config.PanelBackColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# ===== SLIDESHOW SETTINGS =====");
                    writer.WriteLine();
                    writer.WriteLine("# Slideshow interval in seconds (minimum 1, recommended 3-10)");
                    writer.WriteLine($"SlideshowIntervalSeconds={config.SlideshowIntervalSeconds}");
                    writer.WriteLine();
                    writer.WriteLine("# Auto-start slideshow when multiple images are available (true/false)");
                    writer.WriteLine($"AutoStartSlideshow={config.AutoStartSlideshow.ToString().ToLower()}");
                    writer.WriteLine();
                    writer.WriteLine("# ===== DISPLAY SETTINGS =====");
                    writer.WriteLine();
                    writer.WriteLine("# Show 'Open in viewer' link on images (true/false)");
                    writer.WriteLine($"ShowOpenInViewerLink={config.ShowOpenInViewerLink.ToString().ToLower()}");
                    writer.WriteLine();
                    writer.WriteLine("# Default floating window width in pixels");
                    writer.WriteLine($"WindowWidth={config.WindowWidth}");
                    writer.WriteLine();
                    writer.WriteLine("# Default floating window height in pixels");
                    writer.WriteLine($"WindowHeight={config.WindowHeight}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create config file: {ex.Message}", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadConfig()
        {
            try
            {
                string[] lines = File.ReadAllLines(configFilePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                        continue;

                    string[] parts = line.Split('=');
                    if (parts.Length != 2)
                        continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "BackgroundColor":
                            config.BackgroundColor = ParseColor(value);
                            break;
                        case "ForegroundColor":
                            config.ForegroundColor = ParseColor(value);
                            break;
                        case "ButtonBackColor":
                            config.ButtonBackColor = ParseColor(value);
                            break;
                        case "ButtonForeColor":
                            config.ButtonForeColor = ParseColor(value);
                            break;
                        case "ActiveButtonBackColor":
                            config.ActiveButtonBackColor = ParseColor(value);
                            break;
                        case "ActiveButtonForeColor":
                            config.ActiveButtonForeColor = ParseColor(value);
                            break;
                        case "PanelBackColor":
                            config.PanelBackColor = ParseColor(value);
                            break;
                        case "SlideshowIntervalSeconds":
                            if (int.TryParse(value, out int interval) && interval >= 1)
                                config.SlideshowIntervalSeconds = interval;
                            break;
                        case "AutoStartSlideshow":
                            config.AutoStartSlideshow = value.ToLower() == "true";
                            break;
                        case "ShowOpenInViewerLink":
                            config.ShowOpenInViewerLink = value.ToLower() == "true";
                            break;
                        case "WindowWidth":
                            if (int.TryParse(value, out int width) && width > 0)
                                config.WindowWidth = width;
                            break;
                        case "WindowHeight":
                            if (int.TryParse(value, out int height) && height > 0)
                                config.WindowHeight = height;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load config file: {ex.Message}\nUsing default settings.", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Color ParseColor(string colorString)
        {
            try
            {
                string[] rgb = colorString.Split(',');
                if (rgb.Length == 3)
                {
                    int r = int.Parse(rgb[0].Trim());
                    int g = int.Parse(rgb[1].Trim());
                    int b = int.Parse(rgb[2].Trim());
                    return Color.FromArgb(r, g, b);
                }
            }
            catch { }

            return Color.Black;
        }

        public bool Configure(IntPtr panelHandle)
        {
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Panel.FromHandle(panelHandle);

                Label pathLabel = new Label();
                pathLabel.AutoSize = true;
                pathLabel.Location = new Point(0, 0);
                pathLabel.Text = $"Config file: {configFilePath}";
                pathLabel.Font = new Font("Microsoft Sans Serif", 8F);
                pathLabel.MaximumSize = new Size(400, 0);

                Button openConfigButton = new Button();
                openConfigButton.Text = "Open Config in Notepad";
                openConfigButton.Location = new Point(0, 40);
                openConfigButton.Width = 150;
                openConfigButton.Click += (s, e) =>
                {
                    try
                    {
                        System.Diagnostics.Process.Start("notepad.exe", configFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open config file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                Label infoLabel = new Label();
                infoLabel.Location = new Point(0, 70);
                infoLabel.Size = new Size(400, 60);
                infoLabel.Text = "Edit the config file to customize:\n• Colors (background, foreground, buttons)\n• Slideshow interval and auto-start\n• Window size and display options";

                configPanel.Controls.AddRange(new Control[] { pathLabel, openConfigButton, infoLabel });
            }
            return false;
        }

        public void SaveSettings()
        {
        }

        public void Close(PluginCloseReason reason)
        {
            if (albumInsertsForm != null && !albumInsertsForm.IsDisposed)
            {
                albumInsertsForm.Close();
            }
        }

        public void Uninstall()
        {
            if (albumInsertsForm != null && !albumInsertsForm.IsDisposed)
            {
                albumInsertsForm.Close();
            }
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                    break;
                case NotificationType.TrackChanged:
                    if (albumInsertsForm != null && !albumInsertsForm.IsDisposed)
                    {
                        if (albumInsertsForm.InvokeRequired)
                        {
                            albumInsertsForm.Invoke(new Action(() =>
                            {
                                albumInsertsForm.RefreshImagesForCurrentTrack();
                            }));
                        }
                        else
                        {
                            albumInsertsForm.RefreshImagesForCurrentTrack();
                        }
                    }

                    if (dockablePanel != null && !dockablePanel.IsDisposed)
                    {
                        if (dockablePanel.InvokeRequired)
                        {
                            dockablePanel.Invoke(new Action(() =>
                            {
                                dockablePanel.RefreshImagesForCurrentTrack();
                            }));
                        }
                        else
                        {
                            dockablePanel.RefreshImagesForCurrentTrack();
                        }
                    }
                    break;
            }
        }

        private void createMenuItem()
        {
            mbApiInterface.MB_AddMenuItem("mnuView/Album Inserts Viewer", "HotKey", menuClicked);
        }

        private void menuClicked(object sender, EventArgs args)
        {
            if (albumInsertsForm == null || albumInsertsForm.IsDisposed)
            {
                albumInsertsForm = new Form1(mbApiInterface, config);
                albumInsertsForm.FormClosed += (s, e) => albumInsertsForm = null;
                albumInsertsForm.Show();
            }
            else
            {
                albumInsertsForm.BringToFront();
                albumInsertsForm.Focus();
            }
        }

        public int OnDockablePanelCreated(Control panel)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() =>
                {
                    dockablePanel = new AlbumInsertsPanel(mbApiInterface, config);
                    dockablePanel.Dock = DockStyle.Fill;
                    panel.Controls.Add(dockablePanel);
                }));
            }
            else
            {
                dockablePanel = new AlbumInsertsPanel(mbApiInterface, config);
                dockablePanel.Dock = DockStyle.Fill;
                panel.Controls.Add(dockablePanel);
            }

            return -1;
        }

        public List<ToolStripItem> GetHeaderMenuItems()
        {
            List<ToolStripItem> list = new List<ToolStripItem>();

            ToolStripMenuItem openFloatingItem = new ToolStripMenuItem("Open in Floating Window");
            openFloatingItem.Click += menuClicked;
            list.Add(openFloatingItem);

            ToolStripMenuItem openConfigItem = new ToolStripMenuItem("Edit Config");
            openConfigItem.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start("notepad.exe", configFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open config file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            list.Add(openConfigItem);

            ToolStripMenuItem reloadItem = new ToolStripMenuItem("Reload Config");
            reloadItem.Click += (s, e) =>
            {
                LoadConfig();
                if (dockablePanel != null && !dockablePanel.IsDisposed)
                {
                    dockablePanel.ApplyConfig(config);
                }
                MessageBox.Show("Config reloaded! Reopen floating window for changes to take effect.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            list.Add(reloadItem);

            return list;
        }
    }
}