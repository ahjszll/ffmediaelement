using FFmpeg.AutoGen;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.FFME;
using Unosquare.FFME.Common;
using Unosquare.FFME.Container;
using Unosquare.FFME.Primitives;

namespace Unosquare.FFWindowsFormsApp.Core.Decoder
{
    public class FFDecoder : Decoder
    {
        FFME.Engine.MediaEngine _me = new FFME.Engine.MediaEngine();
        public FFDecoder(string file)
        {
            _me.Open(null,new Uri(file));
        }

        public void Start() 
        {
            _me.Play();
            Task.Factory.StartNew(() =>
            {
                while (true) 
                {
                    Thread.Sleep(40);
                    foreach (MediaFrame videoFrame in _me.Frames[MediaType.Video].ReadAll()) 
                    {
                        foreach (var render in _renderers) 
                        {
                            render.RenderVideo(videoFrame,_me.Container);
                        }
                    }
                }
            });
        }
    }
}
