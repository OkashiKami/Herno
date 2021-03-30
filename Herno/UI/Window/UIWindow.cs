using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Veldrid;

namespace Herno.UI
{
    public class UIWindow : UIContainer
    {
        public UIProperty<bool> Open { get; set; }
        public ImGuiWindowFlags Flags { get; set; }

      

        public UIWindow(WindowConfig config, UIProperty<bool> open, ImGuiWindowFlags flags, IEnumerable<IUIComponent> children) : base(children)
        {
            Name = string.IsNullOrEmpty(config.name) ?  "UI Window" : config.name;
            Open = open;
            Flags = flags;
            this.config = config;
        }

        public UIWindow(WindowConfig config, IEnumerable<IUIComponent> children) : this(config, true, children.ToList().Find(x => x.GetType().Equals(typeof(UIMenuBar))) != null ? ImGuiWindowFlags.MenuBar : ImGuiWindowFlags.None, children) { }
        public UIWindow(WindowConfig config) : this(config, Enumerable.Empty<IUIComponent>()) { }

        public override void Render(CommandList cl)
        {
            var open = Open.Value;

            if (!open) return;

            ImGui.Begin(Name, ref open, Flags);
            if (open != Open.Value) Open.Set(open);
            RenderChildren(cl);
            ImGui.End();
        }
    }
}
