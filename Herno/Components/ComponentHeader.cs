using Herno.UI;
using ImGuiNET;
using System;
using System.Numerics;
using Veldrid;

namespace Herno.Components
{
    class ComponentHeader : IUIComponent, IDisposable
    {
        public string Name { get; set; } = "Header";
        
        
        bool _enabled = false;
        string _name = "Temp Object";
        string _layer = "Default";
        string _tag = "Untagged";
        bool _static = false;

        public void Render(CommandList cl)
        {
            ImGui.Checkbox("Enabled", ref _enabled);
            ImGui.SameLine();
            ImGui.Checkbox("Static", ref _static);
            ImGui.InputText("Name", ref _name, 32, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.CallbackCompletion);

            if (ImGui.BeginPopupContextWindow("_tags_"))
            {
                if (ImGui.Selectable("Untagged")) { _tag = "Untagged"; }
               
                ImGui.EndPopup();
            }
            if (ImGui.BeginPopupContextWindow("_layer_"))
            {
                if (ImGui.Selectable("Default")) { _layer = "Default"; }
                if (ImGui.Selectable("Ignore Raycast")) { _layer = "Ignore Raycast"; }
                if (ImGui.Selectable("Walkable")) { _layer = "walkable"; }
                if (ImGui.Selectable("Non-Walkable")) { _layer = "Non-Walkable"; }
                if (ImGui.Selectable("UI")) { _layer = "UI"; }
                ImGui.EndPopup();
            }

            ImGui.Text("Tags");
            ImGui.SameLine();
            if (ImGui.Button(_tag)) { ImGui.OpenPopup("_tags_"); }
            ImGui.SameLine();
            ImGui.Text("Layer");
            ImGui.SameLine();
            if (ImGui.Button(_layer)) { ImGui.OpenPopup("_layer_"); }

            ImGui.Dummy(new Vector2(ImGui.GetWindowContentRegionWidth(), 5));
        }

        protected virtual void ProcessInputs()
        {

        }

        public void Dispose() { }
    }
}
