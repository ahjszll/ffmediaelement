using FFmpeg.AutoGen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Unosquare.FFME.Common;
using Unosquare.FFME.Container;

namespace Unosquare.FFWindowsFormsApp.Core.Renderer
{
    public class Renderer
    {
        protected MediaBlockBuffer _audioBuffer = new MediaBlockBuffer(25, MediaType.Audio);
        protected MediaBlockBuffer _videoBuffer = new MediaBlockBuffer(25, MediaType.Video);

        public virtual void RenderVideo(MediaFrame mediaFrame, MediaContainer container) 
        {
            _videoBuffer.Add(mediaFrame, container);
        }

        public virtual void RenderAudio(MediaFrame mediaFrame, MediaContainer container)
        {
            _audioBuffer.Add(mediaFrame, container);
        }
    }
}
