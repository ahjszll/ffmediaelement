using FFmpeg.AutoGen;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unosquare.FFME;
using Unosquare.FFME.Common;

namespace FFWindowsFormsApp.Core.Decoder
{
    public class FFDecoder : Decoder
    {
        private MediaElement _mediaElement;
        private string _file;

        public FFDecoder(string file)
        {
            _file = file;
            _mediaElement = new MediaElement();
            _mediaElement.MediaOpening += _mediaElement_MediaOpening;
            _mediaElement.MediaOpened += _mediaElement_MediaOpened;
            _mediaElement.VideoFrameDecoded += _mediaElement_VideoFrameDecoded;
            _mediaElement.AudioFrameDecoded += _mediaElement_AudioFrameDecoded;
            _mediaElement.MessageLogged += _mediaElement_MessageLogged;
            _mediaElement.Open(new Uri(file));
        }

        public void Start() 
        {
           
        }

        private void _mediaElement_MessageLogged(object sender, MediaLogMessageEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }

        private void _mediaElement_AudioFrameDecoded(object sender, FrameDecodedEventArgs e)
        {
            unsafe
            {
                lock (_renderers)
                {
                    _renderers.ForEach(r => r.RenderAudio(*e.Frame));
                }
            }
        }

        private void _mediaElement_VideoFrameDecoded(object sender, FrameDecodedEventArgs e)
        {
            unsafe
            {
                lock (_renderers)
                {
                    _renderers.ForEach(r => r.RenderVideo(*e.Frame));
                }
            }
        }

        private void _mediaElement_MediaOpening(object sender, MediaOpeningEventArgs e)
        {
            //var media = sender as MediaElement;
            //if (e.Options.VideoStream is StreamInfo videoStream)
            //{
            //    var deviceCandidates = new[]
            //    {
            //        AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA,
            //        AVHWDeviceType.AV_HWDEVICE_TYPE_QSV,
            //        AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
            //        AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2,
            //    };

            //    foreach (var deviceType in deviceCandidates)
            //    {
            //        var accelerator = videoStream.HardwareDevices.FirstOrDefault(d => d.DeviceType == deviceType);
            //        if (accelerator == null) continue;
            //        e.Options.VideoHardwareDevice = accelerator;
            //        break;
            //    }
            //}
        }

        private void _mediaElement_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            _mediaElement.Play();
        }


        public void Play()
        {
            _mediaElement.Play();
        }
    }
}
