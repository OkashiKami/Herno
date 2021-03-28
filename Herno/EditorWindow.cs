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
                frameTimer.Reset();
                frameTimer.Start();
                imGui.Update((float)frameTimer.Elapsed.TotalSeconds, view.Width, view.Height);

                ImGui.DockSpaceOverViewport();
                cl.Begin();

                // Compute UI elements, render canvases
                uihost.Render(cl);
                //ImGui.ShowDemoWindow();
                hotkeys.Update(true);
                if (hotkeys.CurrentHotkey == GlobalHotkey.Undo) projectConnect.Undo();
                if (hotkeys.CurrentHotkey == GlobalHotkey.Redo) projectConnect.Redo();
                Console.WriteLine(hotkeys.CurrentHotkey);

                //ImGui.Text(ImGui.GetIO().Framerate.ToString());

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
            var mainmenu = new UIMainMenuBar(new IUIComponent[] 
            {
                new UIMenu("File", new IUIComponent[]
                {
                    new UIMenuItem("Open Scene", "CTRL+O"),
                    new UIMenuItem("Open Project", "CTRL+SHIFT+O"),
                    new UIMenuItem("New Scene", "CTRL+N"),
                    new UIMenuItem("New Project", "CTRL+SHIFT+N"),

                    new UIMenuItem("Exit", "ALT+F4", () => view.Close())

                }),
                new UIMenu("Edit", new IUIComponent[]
                {
                    new UIMenuItem("Open Scene", "CTRL+O"),
                    new UIMenuItem("Open Project", "CTRL+SHIFT+O"),
                    new UIMenuItem("New Scene", "CTRL+N"),
                    new UIMenuItem("New Project", "CTRL+SHIFT+N"),

                    new UIMenuItem("Exit", "ALT+F4", () => view.Close())

                }),

                new UIMenu("Windows", new IUIComponent[]
                {
                    new UIMenuItem("Scene", action: () => uihost.Children.Add(new UIWindow("Scene", new IUIComponent[] {  })))

                })
            });

            var sceneWindow = new UIWindow("Scene", new IUIComponent[] 
            { 
            
            });
            var gameWindow = new UIWindow("Game", new IUIComponent[] 
            {
                new UIMenuBar(new IUIComponent[] { new UIMenu("Status", new IUIComponent[] { new UIMenuItem($"FPS: {ImGui.GetIO().Framerate}" )}), }),
            });
            var hierarchyWindow = new UIWindow("Hierarchy", new IUIComponent[]
            { 
            
            });
            var inspectorWindow = new UIWindow("Inspector", new IUIComponent[] 
            {
            
            });
            var projectWindow = new UIWindow("Project", new IUIComponent[] 
            { 
            
            });
            var consoleWindow = new UIWindow("Console", new IUIComponent[]
            { 
            
            });

            var pattern = new MIDIPattern();
            projectConnect = new ProjectConnect();
            var pianoRollWindow = UIUtils.CreatePianoRollWindow(projectConnect, pattern, gd, imGui);

            // Initialize imgui UI
            uihost = disposer.Add(new UIHost(new IUIComponent[] 
            {
                mainmenu, 
                sceneWindow, 
                gameWindow, 
                hierarchyWindow, 
                inspectorWindow,
                projectWindow,
                consoleWindow,
                //pianoRollWindow,
            }));


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
