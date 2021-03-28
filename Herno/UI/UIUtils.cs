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

namespace Herno.UI
{
  public static class UIUtils
  {
    public static UIWindow CreatePianoRollWindow(ProjectConnect projectConnect,  MIDIPattern pattern, GraphicsDevice gd, ImGuiView imGui)
    {
      var menu = new UIMenu("Retard Menu", new IUIComponent[] { new UIMenuItem("Snap Size") });
      var menuBar = new UIMenuBar(new IUIComponent[] { menu });
      pattern.GenNotes();
      var pianoPattern = new MIDIPatternConnect(projectConnect, pattern);
      var canvas = new MIDIPatternIO(gd, imGui, ImGui.GetContentRegionAvail, pianoPattern);
      var window = new UIWindow("PianoRoll", new UIValueProperty<bool>(true), ImGuiWindowFlags.MenuBar, new IUIComponent[] { menuBar, canvas });

      return window;
    }
  }
}
