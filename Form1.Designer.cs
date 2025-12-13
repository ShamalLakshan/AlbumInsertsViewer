namespace MusicBeePlugin
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // Timer
            this.timer1 = new System.Windows.Forms.Timer(this.components);

            // Navigation Panel
            this.navPanel = new System.Windows.Forms.Panel();
            this.btnBooklet = new System.Windows.Forms.Button();
            this.btnScans = new System.Windows.Forms.Button();

            // Content Panel
            this.contentPanel = new System.Windows.Forms.Panel();

            // Scans Panel
            this.scansPanel = new System.Windows.Forms.Panel();
            this.lblOpenImage = new System.Windows.Forms.Label();
            this.txtNoImages = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();

            // Booklet Panel
            this.bookletPanel = new System.Windows.Forms.Panel();
            this.btnLaunchPdf = new System.Windows.Forms.Button();
            this.lblPdfMessage = new System.Windows.Forms.Label();

            this.navPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            this.scansPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.bookletPanel.SuspendLayout();
            this.SuspendLayout();

            // 
            // timer1
            // 
            this.timer1.Interval = 3000;

            // 
            // navPanel
            // 
            this.navPanel.Controls.Add(this.btnBooklet);
            this.navPanel.Controls.Add(this.btnScans);
            this.navPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.navPanel.Location = new System.Drawing.Point(0, 0);
            this.navPanel.Name = "navPanel";
            this.navPanel.Size = new System.Drawing.Size(800, 40);
            this.navPanel.TabIndex = 0;

            // 
            // btnBooklet
            // 
            this.btnBooklet.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBooklet.FlatAppearance.BorderSize = 0;
            this.btnBooklet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBooklet.Location = new System.Drawing.Point(100, 0);
            this.btnBooklet.Name = "btnBooklet";
            this.btnBooklet.Size = new System.Drawing.Size(100, 40);
            this.btnBooklet.TabIndex = 1;
            this.btnBooklet.Text = "Booklet";
            this.btnBooklet.UseVisualStyleBackColor = true;

            // 
            // btnScans
            // 
            this.btnScans.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnScans.FlatAppearance.BorderSize = 0;
            this.btnScans.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScans.Location = new System.Drawing.Point(0, 0);
            this.btnScans.Name = "btnScans";
            this.btnScans.Size = new System.Drawing.Size(100, 40);
            this.btnScans.TabIndex = 0;
            this.btnScans.Text = "Scans";
            this.btnScans.UseVisualStyleBackColor = true;

            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.scansPanel);
            this.contentPanel.Controls.Add(this.bookletPanel);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(0, 40);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(800, 560);
            this.contentPanel.TabIndex = 1;

            // 
            // scansPanel
            // 
            this.scansPanel.Controls.Add(this.lblOpenImage);
            this.scansPanel.Controls.Add(this.txtNoImages);
            this.scansPanel.Controls.Add(this.pictureBox1);
            this.scansPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scansPanel.Location = new System.Drawing.Point(0, 0);
            this.scansPanel.Name = "scansPanel";
            this.scansPanel.Size = new System.Drawing.Size(800, 560);
            this.scansPanel.TabIndex = 0;

            // 
            // lblOpenImage
            // 
            this.lblOpenImage.AutoSize = true;
            this.lblOpenImage.BackColor = System.Drawing.Color.Transparent;
            this.lblOpenImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblOpenImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Underline);
            this.lblOpenImage.Location = new System.Drawing.Point(680, 520);
            this.lblOpenImage.Name = "lblOpenImage";
            this.lblOpenImage.Size = new System.Drawing.Size(100, 13);
            this.lblOpenImage.TabIndex = 2;
            this.lblOpenImage.Text = "🔗 Open in viewer";
            this.lblOpenImage.Visible = false;

            // 
            // txtNoImages
            // 
            this.txtNoImages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtNoImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNoImages.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtNoImages.Location = new System.Drawing.Point(0, 0);
            this.txtNoImages.Multiline = true;
            this.txtNoImages.Name = "txtNoImages";
            this.txtNoImages.ReadOnly = true;
            this.txtNoImages.Size = new System.Drawing.Size(800, 560);
            this.txtNoImages.TabIndex = 1;
            this.txtNoImages.Text = "No images found";
            this.txtNoImages.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtNoImages.Visible = false;

            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(800, 560);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;

            // 
            // bookletPanel
            // 
            this.bookletPanel.Controls.Add(this.btnLaunchPdf);
            this.bookletPanel.Controls.Add(this.lblPdfMessage);
            this.bookletPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bookletPanel.Location = new System.Drawing.Point(0, 0);
            this.bookletPanel.Name = "bookletPanel";
            this.bookletPanel.Size = new System.Drawing.Size(800, 560);
            this.bookletPanel.TabIndex = 1;
            this.bookletPanel.Visible = false;

            // 
            // btnLaunchPdf
            // 
            this.btnLaunchPdf.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLaunchPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLaunchPdf.Location = new System.Drawing.Point(20, 120);
            this.btnLaunchPdf.Name = "btnLaunchPdf";
            this.btnLaunchPdf.Size = new System.Drawing.Size(760, 35);
            this.btnLaunchPdf.TabIndex = 1;
            this.btnLaunchPdf.Text = "Launch in External Viewer";
            this.btnLaunchPdf.UseVisualStyleBackColor = true;
            this.btnLaunchPdf.Visible = false;

            // 
            // lblPdfMessage
            // 
            this.lblPdfMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPdfMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblPdfMessage.Location = new System.Drawing.Point(0, 0);
            this.lblPdfMessage.Name = "lblPdfMessage";
            this.lblPdfMessage.Size = new System.Drawing.Size(800, 100);
            this.lblPdfMessage.TabIndex = 0;
            this.lblPdfMessage.Text = "No PDF detected";
            this.lblPdfMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPdfMessage.Visible = false;

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.navPanel);
            this.Name = "Form1";
            this.Text = "Album Inserts Viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.navPanel.ResumeLayout(false);
            this.contentPanel.ResumeLayout(false);
            this.scansPanel.ResumeLayout(false);
            this.scansPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.bookletPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel navPanel;
        private System.Windows.Forms.Button btnScans;
        private System.Windows.Forms.Button btnBooklet;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Panel scansPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtNoImages;
        private System.Windows.Forms.Label lblOpenImage;
        private System.Windows.Forms.Panel bookletPanel;
        private System.Windows.Forms.Label lblPdfMessage;
        private System.Windows.Forms.Button btnLaunchPdf;
    }
}