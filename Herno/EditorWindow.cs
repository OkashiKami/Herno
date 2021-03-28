using System;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using ImGuiNET;
using Herno.UI;
using Herno.Renderer;
using System.Diagnostics;
using Herno.Components;
using Herno.MIDI;
using Herno.Components.Project;
using Herno.Components.Common.Hotkeys;
using System.Reflection;

namespace Herno
{
    using ImGui = ImGuiNET.ImGui;
    class EditorWindow : IDisposable
    {
        private Vector3 clearColor = new Vector3(0.10f, 0.10f, 0.10f);
        DisposeGroup disposer;
        private RenderView view;
        private GraphicsDevice gd;
        private CommandList cl;
        private ImGuiView imGui;
        private UIHost uihost;
        private static Stopwatch frameTimer;
        private HotkeyHandler<GlobalHotkey> hotkeys;
        private ProjectConnect projectConnect;

        /// <summary>
		/// Create and initialize a new game.
		/// </summary>
        public EditorWindow()
        {
            disposer = new DisposeGroup();
            // Initialize window and imgui
            var projectInfo = Assembly.GetExecutingAssembly().GetName();
            view = disposer.Add(new RenderView(new RenderViewSettings() 
            { 
                X = 100,
                Y = 100,
                Width = 1280,
                Height = 720,
                Title = $"{projectInfo.Name} {DateTime.Now.Year}.{projectInfo.Version.Major}.{projectInfo.Version.Minor}.r{projectInfo.Version.Revision}",
            }));
            gd = view.GraphicsDevice;
            cl = gd.ResourceFactory.CreateCommandList();
            imGui = new ImGuiView(gd, view.Window, gd.MainSwapchain.Framebuffer.OutputDescription, view.Width, view.Height);
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
        }

        /// <summary>
		/// Start the game.
		/// </summary>
        public void Run()
        {
            InitializeUi();

            while (view.Exists)
            {
                if (!view.Exists) { break; }
                imGui.Update((float)frameTimer.Elapsed.TotalSeconds, view.Width, view.Height);
                frameTimer.Reset();
                frameTimer.Start();

                cl.Begin();

                // Compute UI elements, render canvases
                ImGui.DockSpaceOverViewport();
                uihost.Render(cl);
                //ImGui.ShowDemoWindow();
                hotkeys.Update(true);
                if (hotkeys.CurrentHotkey == GlobalHotkey.Undo) projectConnect.Undo();
                if (hotkeys.CurrentHotkey == GlobalHotkey.Redo) projectConnect.Redo();
                Console.WriteLine(hotkeys.CurrentHotkey);

                ImGui.Text(ImGui.GetIO().Framerate.ToString());

                imGui.UpdateViewIO(view);

                cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
                cl.ClearColorTarget(0, new RgbaFloat(clearColor.X, clearColor.Y, clearColor.Z, 1f));
                imGui.Render(gd, cl);
                cl.End();
                gd.SubmitCommands(cl);
                gd.SwapBuffers(gd.MainSwapchain);
                imGui.SwapExtraWindows(gd);
            }
        }

        private void InitializeUi()
        {
            //Create ImGui Windows
            var menu = new UIMenu("Hello World!", new IUIComponent[] 
            {
                new UIMenuItem("Test 1"),
                new UIMenuItem("Test 2", "CTRL+Z"),
                new UIMenuItem("Window", action: () => uihost.Children.Add(new UIWindow("Dynamic ImGui!", new IUIComponent[] { new UIText("Test Text"), new UICheckbox("Test Checkbox", false) })))

            });
            var mainmenu = new UIMainMenuBar(new IUIComponent[] { menu });

            var uiwindow = new UIWindow("Abstracted ImGui!", new IUIComponent[] { new UIText("Test Text"), new UICheckbox("Test Checkbox", false) });

            var pattern = new MIDIPattern();
            projectConnect = new ProjectConnect();
            var pianoRollWindow = UIUtils.CreatePianoRollWindow(projectConnect, pattern, gd, imGui);

            // Initialize imgui UI
            uihost = disposer.Add(new UIHost(new IUIComponent[] { mainmenu, uiwindow, pianoRollWindow }));


            frameTimer = new Stopwatch();
            frameTimer.Start();

            hotkeys = new HotkeyHandler<GlobalHotkey>();
        }


        public void Dispose()
        {
            // Clean up Veldrid resources
            disposer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
} 
