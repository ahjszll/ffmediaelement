using FFmpeg.AutoGen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace FFWindowsFormsApp.Core.Renderer
{
    public class Renderer
    {
        protected ConcurrentQueue<AVFrame> _audioQueue = new ConcurrentQueue<AVFrame>();
        protected ConcurrentQueue<AVFrame> _videoQueue = new ConcurrentQueue<AVFrame>();

        public virtual void RenderVideo(AVFrame frame) 
        {
            _videoQueue.Enqueue(frame);
        }

        public virtual void RenderAudio(AVFrame frame)
        {
            _audioQueue.Enqueue(frame);
        }
    }
}
