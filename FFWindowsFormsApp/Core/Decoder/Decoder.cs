using System.Collections.Generic;

namespace FFWindowsFormsApp.Core.Decoder
{
    public class Decoder
    {
        public List<Renderer.Renderer> _renderers = new List<Renderer.Renderer>();

        public void AddRenderer(Renderer.Renderer renderer) 
        {
            lock (_renderers) 
            {
                _renderers.Add(renderer);
            }
        }

        public void RemoveRenderer(Renderer.Renderer renderer)
        {
            lock (_renderers)
            {
                if (_renderers.Contains(renderer))
                {
                    _renderers.Remove(renderer);
                }
            }
        }
    }
}
