using Herno.Renderer;
using Herno.UI;
using Veldrid;

namespace Herno
{
    public struct WindowConfig
    {
        public string name { get; set; }
        public GraphicsDevice device { get; set; }
        public ImGuiView view { get; set; }
        public RgbaFloat color { get; set; }
        public IUIComponent[] components { get; set; }

    }
}