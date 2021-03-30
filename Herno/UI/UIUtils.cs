using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Herno.Components;
using Herno.Components.PianoRoll;
using Herno.Components.Project;
using Herno.MIDI;
using Herno.Renderer;
using Veldrid;
using Viewport = Herno.Components.Viewport;

namespace Herno.UI
{
    public static class UIUtils
    {
        public static UIWindow CreatePianoRollWindow(ProjectConnect projectConnect, MIDIPattern pattern, GraphicsDevice gd, ImGuiView view)
        {
            var menu = new UIMenu("Retard Menu", new IUIComponent[] { new UIMenuItem("Snap Size") });
            var menuBar = new UIMenuBar(new IUIComponent[] { menu });
            pattern.GenNotes();
            var pianoPattern = new MIDIPatternConnect(projectConnect, pattern);
            var canvas = new MIDIPatternIO(gd, view, ImGui.GetContentRegionAvail, pianoPattern);
            
            var window = new UIWindow( new WindowConfig() 
            {
                name = "PianoRoll",
                device = gd,
                view = view,
            }, new UIValueProperty<bool>(true), ImGuiWindowFlags.MenuBar, new IUIComponent[] { menuBar, canvas });

            return window;
        }

        internal static UIWindow CreateWindow(WindowConfig config, params IUIComponent[] components)
        {
            var canvas = new Viewport(config, ImGui.GetContentRegionAvail);
            var com = new List<IUIComponent>();
            com.Add(canvas);
            com.AddRange(components);
            var window = new UIWindow(config, new UIValueProperty<bool>(true), ImGuiWindowFlags.None, com.ToArray());
            return window;
        }
    }
}
