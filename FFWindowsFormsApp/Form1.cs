using FFWindowsFormsApp.Core;
using FFWindowsFormsApp.Core.Package;
using System;
using System.Windows.Forms;

namespace FFWindowsFormsApp
{
    public partial class Form1 : Form
    {

        Panel video1 = new Panel();
        Panel video2 = new Panel();

        MediaBox mb = new MediaBox();

        public Form1()
        {
            InitializeComponent();
            this.Controls.Add(video1);
            this.Controls.Add(video2);
            this.WindowState = FormWindowState.Maximized;
            video1.Dock = DockStyle.Left;
            video1.Width = 1920 / 2;
            video2.Dock = DockStyle.Right;
            video2.Width = 1920 / 2;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FFDecoder decoder = new FFDecoder();
            decoder.Open("f:\\test.mpg");
            decoder.Play();
            mb.AddFFDecoder(decoder);

            SDL2VideoRenderer renderer = new SDL2VideoRenderer();
            renderer.SetHandle(video1.Handle);
            mb.AddRenderer(renderer);

            mb.Start();
            mb.Connect();
        }
    }
}
