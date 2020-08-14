using System;
using System.Timers;

namespace Unosquare.FFWindowsFormsApp.Core.Renderer
{
    public class SDL2AudioRenderer : Renderer
    {
        private object _sdlLocker = new object();
        private Timer _timer;

        public SDL2AudioRenderer()
        {
            InitSDL();
            _timer = new Timer();
            _timer.Interval = 20;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
           
        }

        private void InitSDL()
        {
            if (SDL2VideoRenderer.SdlInit)
                return;
            SDL2.SDL.SDL_Init(SDL2.SDL.SDL_INIT_AUDIO | SDL2.SDL.SDL_INIT_VIDEO);
            SDL2.SDL.SDL_EventState(SDL2.SDL.SDL_EventType.SDL_WINDOWEVENT, SDL2.SDL.SDL_IGNORE);
            SDL2VideoRenderer.SdlInit = true;
        }


        private void CreateSDL()
        {
            lock (_sdlLocker)
            {

            }
        }

        private void DisposeSDL()
        {
            lock (_sdlLocker)
            {
             
            }
        }

        public void Dispose()
        {
            DisposeSDL();
        }
    }
}
