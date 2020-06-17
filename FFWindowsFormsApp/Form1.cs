using FFWindowsFormsApp.Core.Decoder;
using FFWindowsFormsApp.Core.Renderer;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFWindowsFormsApp
{
    public partial class Form1 : Form
    {

        Panel video1 = new Panel();
        Panel video2 = new Panel();

        FFDecoder _decoder = null;

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
            _decoder = new FFDecoder("e:\\电影模式.mp4");
            SDL2VideoRenderer renderer = new SDL2VideoRenderer();
            renderer.SetHandle(video1.Handle);
            _decoder.AddRenderer(renderer);

            Task.Factory.StartNew(() => 
            {
                while (true) 
                {
                    Thread.Sleep(3000);
                    this.Invoke(new MethodInvoker(() => { 
                        renderer.SetHandle(video2.Handle);
                    }));
                    Thread.Sleep(3000);
                    this.Invoke(new MethodInvoker(() => {
                        renderer.SetHandle(video1.Handle);
                    }));
                    
                }
            });
        }
    }
}
