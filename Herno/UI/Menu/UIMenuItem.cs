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
        public UIMenuItem(string label, string? shortcut = null, Action action = null, bool readOnly = false)
        {
            Label = label;
            Shortcut = shortcut;
            Action = action;
            ReadOnly = readOnly;
        }

        public string Label { get; set; }
        public string? Shortcut { get; set; }
        public Action? Action { get; set; }
        public bool ReadOnly { get; set; }

        public override void Render(CommandList cl)
        {
            if (ReadOnly)
                ImGui.MenuItem(Label,  !ReadOnly);
            else
                if (ImGui.MenuItem(Label, Shortcut)) { Action?.Invoke(); }
        }
    }
}