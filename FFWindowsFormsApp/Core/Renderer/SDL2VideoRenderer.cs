using System;
using System.Windows.Forms;
using Unosquare.FFME.Container;
namespace FFWindowsFormsApp.Core.Package
{
    public class SDL2VideoRenderer : Renderer
    {
        private IntPtr _windowPtr;
        private IntPtr _rendererPtr;
        private IntPtr _texture;
        private object _sdlLocker = new object();
        private SDL2.SDL.SDL_Rect _rect = new SDL2.SDL.SDL_Rect() { x = 0, y = 0, w = 0, h = 0 };
        private Timer _timer = new Timer();

        private IntPtr _curVideoHandle;
        private IntPtr _newVideoHandle;

        private bool _thisSdlInit = false;

        public static bool GlobalSdlInit = false;



        public SDL2VideoRenderer()
        {
            InitSDL();
            _timer.Interval = 40;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (PlaybackVideoBlock.Count > 0) 
            {
                VideoBlock videoBlock = null;
                PlaybackVideoBlock.TryDequeue(out videoBlock);
                Render(videoBlock);
                PoolVideoBlock.Enqueue(videoBlock);
            }
        }

        private void InitSDL()
        {
            if (GlobalSdlInit)
                return;
            SDL2.SDL.SDL_Init(SDL2.SDL.SDL_INIT_AUDIO | SDL2.SDL.SDL_INIT_VIDEO);
            SDL2.SDL.SDL_EventState(SDL2.SDL.SDL_EventType.SDL_WINDOWEVENT, SDL2.SDL.SDL_IGNORE);
            GlobalSdlInit = true;
        }



        public void SetHandle(IntPtr videoHandle)
        {
            _newVideoHandle = videoHandle;
        }

        private void Render(VideoBlock block)
        {
            lock (_sdlLocker)
            {
                if (_thisSdlInit == false || block.PixelWidth != _rect.w || block.PixelHeight != _rect.h || _newVideoHandle != _curVideoHandle)
                {
                    _rect.w = block.PixelWidth;
                    _rect.h = block.PixelHeight;
                    InitThisSDL(_newVideoHandle, block.PixelWidth, block.PixelHeight);
                }
                unsafe
                {
                    if (block.BufferLength > 0)
                    {
                        SDL2.SDL.SDL_UpdateTexture(_texture, ref _rect, block.Buffer, block.PictureBufferStride);
                        SDL2.SDL.SDL_RenderCopy(_rendererPtr, _texture, IntPtr.Zero, IntPtr.Zero);
                        SDL2.SDL.SDL_RenderPresent(_rendererPtr);
                    }
                }
            }
        }

        private void InitThisSDL(IntPtr videoHandle, int width, int height)
        {
            lock (_sdlLocker)
            {
                if (_thisSdlInit)
                {
                    DisposeThisSDL();
                }
                _windowPtr = SDL2.SDL.SDL_CreateWindowFrom(videoHandle);
                _curVideoHandle = videoHandle;
                SDL2.SDL.SDL_ShowWindow(_windowPtr);//destroyWindows需要show出来
                _rendererPtr = SDL2.SDL.SDL_CreateRenderer(_windowPtr, -1, SDL2.SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
                _texture = SDL2.SDL.SDL_CreateTexture(_rendererPtr, SDL2.SDL.SDL_PIXELFORMAT_ARGB8888, (int)SDL2.SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, width, height);
                _thisSdlInit = true;
            }
        }

        private void DisposeThisSDL()
        {
            lock (_sdlLocker)
            {
                if (_thisSdlInit)
                {
                    SDL2.SDL.SDL_DestroyTexture(_texture);
                    SDL2.SDL.SDL_DestroyRenderer(_rendererPtr);
                    SDL2.SDL.SDL_DestroyWindow(_windowPtr);
                    _texture = IntPtr.Zero;
                    _rendererPtr = IntPtr.Zero;
                    _windowPtr = IntPtr.Zero;
                    _curVideoHandle = IntPtr.Zero;
                    _thisSdlInit = false;
                }
            }
        }

        public void Dispose()
        {
            DisposeThisSDL();
        }
    }
}
