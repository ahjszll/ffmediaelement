using FFmpeg.AutoGen;
using System;
using System.Windows.Forms;
using Unosquare.FFME.Common;
using Unosquare.FFME.Container;

namespace Unosquare.FFWindowsFormsApp.Core.Renderer
{
    public class SDL2VideoRenderer : Renderer
    {
        private IntPtr _windowPtr;
        private IntPtr _rendererPtr;
        private IntPtr _texture;
        private object _sdlLocker = new object();
        private SDL2.SDL.SDL_Rect _rect = new SDL2.SDL.SDL_Rect() { x = 0, y = 0, w = 1920, h = 1080 };
        private Timer _timer = new Timer();

        public static bool SdlInit = false;


        public SDL2VideoRenderer()
        {
            InitSDL();
            _timer.Interval = 40;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            //if (_videoQueue.Count > 0)
            //{
            //    _videoQueue.TryDequeue(out MediaFrame frame);
            //    if (frame != null)
            //    {
            //        //Render(frame.);
            //        frame.Dispose();
            //    }
            //}
        }

        private void InitSDL()
        {
            if (SdlInit)
                return;
            SDL2.SDL.SDL_Init(SDL2.SDL.SDL_INIT_AUDIO | SDL2.SDL.SDL_INIT_VIDEO);
            SDL2.SDL.SDL_EventState(SDL2.SDL.SDL_EventType.SDL_WINDOWEVENT, SDL2.SDL.SDL_IGNORE);
            SdlInit = true;
        }

        public void SetHandle(IntPtr videoHandle)
        {
            DisposeSDL();
            lock (_sdlLocker)
            {
                _windowPtr = SDL2.SDL.SDL_CreateWindowFrom(videoHandle);
                SDL2.SDL.SDL_ShowWindow(_windowPtr);//destroyWindows需要show出来
                _rendererPtr = SDL2.SDL.SDL_CreateRenderer(_windowPtr, -1, SDL2.SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
                _texture = SDL2.SDL.SDL_CreateTexture(_rendererPtr, SDL2.SDL.SDL_PIXELFORMAT_IYUV, (int)SDL2.SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 1920, 1080);
            }
        }

        private void Render(AVFrame frame)
        {
            lock (_sdlLocker)
            {
                unsafe
                {
                    if (frame.width != _rect.w || frame.height != _rect.h)
                    {
                        _rect = new SDL2.SDL.SDL_Rect() { x = 0, y = 0, w = frame.width, h = frame.height };
                    }
                    SDL2.SDL.SDL_UpdateYUVTexture(_texture, ref _rect, (IntPtr)(frame.data[0]), 1920, (IntPtr)(frame.data[1]), 1920 / 2, (IntPtr)(frame.data[2]), 1920 / 2);
                    SDL2.SDL.SDL_RenderCopy(_rendererPtr, _texture, IntPtr.Zero, IntPtr.Zero);
                    SDL2.SDL.SDL_RenderPresent(_rendererPtr);
                }
            }
        }

        private void DisposeSDL()
        {
            lock (_sdlLocker)
            {
                SDL2.SDL.SDL_DestroyTexture(_texture);
                SDL2.SDL.SDL_DestroyRenderer(_rendererPtr);
                SDL2.SDL.SDL_DestroyWindow(_windowPtr);
                _texture = IntPtr.Zero;
                _rendererPtr = IntPtr.Zero;
                _windowPtr = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            DisposeSDL();
        }
    }
}
