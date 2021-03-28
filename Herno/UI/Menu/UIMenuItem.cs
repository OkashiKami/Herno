using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Veldrid;

namespace Herno.UI
{
    public class UIMenuItem : UIComponent
    {
        public UIMenuItem(string label, string? shortcut = null, Action action = null)
        {
            Label = label;
            Shortcut = shortcut;
            Action = action;
        }

        public string Label { get; set; }
        public string? Shortcut { get; set; }
        public Action? Action { get; set; }

        public override void Render(CommandList cl)
        {
            if (ImGui.MenuItem(Label, Shortcut)) { Action?.Invoke(); }
        }
    }
}