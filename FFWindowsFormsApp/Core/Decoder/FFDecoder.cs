using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.FFME.Common;
using Unosquare.FFME.Container;

namespace FFWindowsFormsApp.Core.Package
{
    public class FFDecoder: MediaNode
    {
        private Unosquare.FFME.Engine.MediaEngine _me = null;
        private HardwareDeviceInfo _haInfo = null;
     

        public FFDecoder()
        {
            _me = new Unosquare.FFME.Engine.MediaEngine();
        }

        public void Open(string file)
        {
            _me.Open(null, new Uri(file), _haInfo);
        }

        public void UseHW()
        {
            _haInfo = null;
            foreach (var haa in HardwareAccelerator.GetCompatibleDevices(AVCodecID.AV_CODEC_ID_H264))
            {
                if (haa.DeviceType == AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA)
                {
                    _haInfo = haa;
                }
            }
        }

        public void Play()
        {
            _me.Play();
        }

        public void Stop()
        { 
        
        }

        public void Close() 
        { 
        
        }

        public List<MediaFrame> ReadVideoMediaFrame()
        {
            return _me.Frames[MediaType.Video].ReadAll();
        }

        public List<MediaFrame> ReadAudioMediaFrame() 
        {
            return _me.Frames[MediaType.Audio].ReadAll();
        }


    }
}
