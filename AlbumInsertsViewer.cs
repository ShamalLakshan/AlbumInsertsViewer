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
        private ThemeColors themeColors;

        public class ThemeColors
        {
            public Color BackgroundColor { get; set; }
            public Color ForegroundColor { get; set; }
            public Color TabControlBackColor { get; set; }
            public Color TabPageBackColor { get; set; }
            public Color ButtonBackColor { get; set; }
            public Color ButtonForeColor { get; set; }

            public ThemeColors()
            {
                // Default: Everything black
                BackgroundColor = Color.Black;
                ForegroundColor = Color.White;
                TabControlBackColor = Color.Black;
                TabPageBackColor = Color.Black;
                ButtonBackColor = Color.FromArgb(30, 30, 30);
                ButtonForeColor = Color.White;
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
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;

            // Initialize config file path
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            configFilePath = Path.Combine(dataPath, "albuminsertsviewer.colors.conf");

            // Load or create color configuration
            LoadOrCreateColorConfig();

            createMenuItem();
            return about;
        }

        private void LoadOrCreateColorConfig()
        {
            themeColors = new ThemeColors();

            if (!File.Exists(configFilePath))
            {
                // Create default config file
                CreateDefaultColorConfig();
            }
            else
            {
                // Load existing config
                LoadColorConfig();
            }
        }

        private void CreateDefaultColorConfig()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(configFilePath))
                {
                    writer.WriteLine("# Album Inserts Viewer Color Configuration");
                    writer.WriteLine("# Colors are in R,G,B format (0-255 for each component)");
                    writer.WriteLine("# Edit these values to match your MusicBee theme");
                    writer.WriteLine();
                    writer.WriteLine("# Main background color");
                    writer.WriteLine($"BackgroundColor={themeColors.BackgroundColor.R},{themeColors.BackgroundColor.G},{themeColors.BackgroundColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Main text/foreground color");
                    writer.WriteLine($"ForegroundColor={themeColors.ForegroundColor.R},{themeColors.ForegroundColor.G},{themeColors.ForegroundColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Tab control background");
                    writer.WriteLine($"TabControlBackColor={themeColors.TabControlBackColor.R},{themeColors.TabControlBackColor.G},{themeColors.TabControlBackColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Tab page background");
                    writer.WriteLine($"TabPageBackColor={themeColors.TabPageBackColor.R},{themeColors.TabPageBackColor.G},{themeColors.TabPageBackColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Button background");
                    writer.WriteLine($"ButtonBackColor={themeColors.ButtonBackColor.R},{themeColors.ButtonBackColor.G},{themeColors.ButtonBackColor.B}");
                    writer.WriteLine();
                    writer.WriteLine("# Button text color");
                    writer.WriteLine($"ButtonForeColor={themeColors.ButtonForeColor.R},{themeColors.ButtonForeColor.G},{themeColors.ButtonForeColor.B}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create color config file: {ex.Message}", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadColorConfig()
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

                    Color color = ParseColor(value);

                    switch (key)
                    {
                        case "BackgroundColor":
                            themeColors.BackgroundColor = color;
                            break;
                        case "ForegroundColor":
                            themeColors.ForegroundColor = color;
                            break;
                        case "TabControlBackColor":
                            themeColors.TabControlBackColor = color;
                            break;
                        case "TabPageBackColor":
                            themeColors.TabPageBackColor = color;
                            break;
                        case "ButtonBackColor":
                            themeColors.ButtonBackColor = color;
                            break;
                        case "ButtonForeColor":
                            themeColors.ButtonForeColor = color;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load color config file: {ex.Message}\nUsing default colors.", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                Label prompt = new Label();
                prompt.AutoSize = true;
                prompt.Location = new Point(0, 0);
                prompt.Text = $"Color config file: {configFilePath}";
                prompt.Font = new Font("Microsoft Sans Serif", 8F);

                Button openConfigButton = new Button();
                openConfigButton.Text = "Open Color Config";
                openConfigButton.Location = new Point(0, 25);
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

                configPanel.Controls.AddRange(new Control[] { prompt, openConfigButton });
            }
            return false;
        }

        public void SaveSettings()
        {
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
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
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
                        case PlayState.Paused:
                            break;
                    }
                    break;
                case NotificationType.TrackChanged:
                    string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);

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
                case NotificationType.PlayStateChanged:
                    break;
                case NotificationType.TagsChanged:
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
                albumInsertsForm = new Form1(mbApiInterface);
                ApplyThemeToForm(albumInsertsForm);
                albumInsertsForm.FormClosed += (s, e) => albumInsertsForm = null;
                albumInsertsForm.Show();
            }
            else
            {
                albumInsertsForm.BringToFront();
                albumInsertsForm.Focus();
            }
        }

        private void ApplyThemeToForm(Form1 form)
        {
            form.BackColor = themeColors.BackgroundColor;
            form.ForeColor = themeColors.ForegroundColor;
            ApplyThemeToControls(form.Controls);
        }

        private void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is TabControl)
                {
                    control.BackColor = themeColors.TabControlBackColor;
                    control.ForeColor = themeColors.ForegroundColor;

                    TabControl tabControl = (TabControl)control;
                    foreach (TabPage page in tabControl.TabPages)
                    {
                        page.BackColor = themeColors.TabPageBackColor;
                        page.ForeColor = themeColors.ForegroundColor;
                        ApplyThemeToControls(page.Controls);
                    }
                }
                // Explicit casting to button to preserve FlatStyle properties.
                else if (control is Button button)
                {
                    button.BackColor = themeColors.ButtonBackColor;
                    button.ForeColor = themeColors.ButtonForeColor;
                    button.FlatStyle = FlatStyle.Flat;
                }
                else if (control is PictureBox)
                {
                    control.BackColor = themeColors.BackgroundColor;
                }
                else
                {
                    control.BackColor = themeColors.BackgroundColor;
                    control.ForeColor = themeColors.ForegroundColor;
                }

                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        public int OnDockablePanelCreated(Control panel)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() =>
                {
                    dockablePanel = new AlbumInsertsPanel(mbApiInterface);
                    dockablePanel.Dock = DockStyle.Fill;
                    panel.Controls.Add(dockablePanel);

                    dockablePanel.BackColor = themeColors.BackgroundColor;
                    dockablePanel.ForeColor = themeColors.ForegroundColor;
                    ApplyThemeToControls(dockablePanel.Controls);
                }));
            }
            else
            {
                dockablePanel = new AlbumInsertsPanel(mbApiInterface);
                dockablePanel.Dock = DockStyle.Fill;
                panel.Controls.Add(dockablePanel);

                dockablePanel.BackColor = themeColors.BackgroundColor;
                dockablePanel.ForeColor = themeColors.ForegroundColor;
                ApplyThemeToControls(dockablePanel.Controls);
            }

            return -1;
        }

        public List<ToolStripItem> GetHeaderMenuItems()
        {
            List<ToolStripItem> list = new List<ToolStripItem>();

            ToolStripMenuItem openFloatingItem = new ToolStripMenuItem("Open in Floating Window");
            openFloatingItem.Click += menuClicked;
            list.Add(openFloatingItem);

            ToolStripMenuItem openConfigItem = new ToolStripMenuItem("Edit Colors Config");
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

            ToolStripMenuItem reloadColorsItem = new ToolStripMenuItem("Reload Colors");
            reloadColorsItem.Click += (s, e) =>
            {
                LoadColorConfig();
                if (dockablePanel != null && !dockablePanel.IsDisposed)
                {
                    dockablePanel.BackColor = themeColors.BackgroundColor;
                    dockablePanel.ForeColor = themeColors.ForegroundColor;
                    ApplyThemeToControls(dockablePanel.Controls);
                    dockablePanel.Refresh();
                }
                MessageBox.Show("Colors reloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            list.Add(reloadColorsItem);

            return list;
        }
    }
}