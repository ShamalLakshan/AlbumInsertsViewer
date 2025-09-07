using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //webBrowser1.Url = new Uri("E:\\booklet.pdf");
            
            // Image List Usage
            //Graphics g = Graphics.FromHwnd(Handle);
            //imageList1.ColorDepth = ColorDepth.Depth32Bit;
            //imageList1.ImageSize = new Size(255, 255);

            //imageList1.Images.Add(Image.FromFile(@"E:\booklet_01.jpg"));
            //imageList1.Images.Add(Image.FromFile(@"E:\CD3-5-SleepingBeauty-front.jpg"));
            //imageList1.Images.Add(Image.FromFile(@"E:\CD6_CD7-Nutcracker-front.jpg"));

            //for (int i = 0; i < imageList1.Images.Count;i++)
            //{
            //    imageList1.Draw(g, new Point(40, 40), i);
            //    System.Threading.Thread.Sleep(2000);
            //}

            //Use picture box()

        }

        private void button1_Click(object sender, EventArgs e)
        {
            lblState.ForeColor = Color.Red;
        }

    }
}
