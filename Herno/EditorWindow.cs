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
        private ImGuiView imguiView;
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
            imguiView = new ImGuiView(gd, view.Window, gd.MainSwapchain.Framebuffer.OutputDescription, view.Width, view.Height);
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
                imguiView.Update((float)frameTimer.Elapsed.TotalSeconds, view.Width, view.Height);
                ImGui.DockSpaceOverViewport();
                cl.Begin();

                // Compute UI elements, render canvases
                uihost.Render(cl);
                hotkeys.Update(true);
                if (hotkeys.CurrentHotkey == GlobalHotkey.Undo) projectConnect.Undo();
                if (hotkeys.CurrentHotkey == GlobalHotkey.Redo) projectConnect.Redo();
                Console.WriteLine(hotkeys.CurrentHotkey);

                //ImGui.Text(ImGui.GetIO().Framerate.ToString());

                imguiView.UpdateViewIO(view);

                cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
                cl.ClearColorTarget(0, new RgbaFloat(clearColor.X, clearColor.Y, clearColor.Z, 1f));
                imguiView.Render(gd, cl);
                cl.End();
                gd.SubmitCommands(cl);
                gd.SwapBuffers(gd.MainSwapchain);
                imguiView.SwapExtraWindows(gd);
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

                    new UIMenuItem("Exit", "ALT+F4", () => view.Close()),

                }),
                new UIMenu("Edit", new IUIComponent[]
                {
                    new UIMenuItem("Undo", "CTRL+Z"),
                    new UIMenuItem("Redo", "CTRL+SHIFT+Z"),
                    new UIMenuItem("Cut", "CTRL+X"),
                    new UIMenuItem("Copy", "CTRL+C"),
                    new UIMenuItem("Paste", "CTRL+V"),
                }),
                new UIMenu("View", new IUIComponent[]
                {
                    new UIMenuItem("Undo", "CTRL+Z"),
                    new UIMenuItem("Redo", "CTRL+SHIFT+Z"),
                    new UIMenuItem("Cut", "CTRL+X"),
                    new UIMenuItem("Copy", "CTRL+C"),
                    new UIMenuItem("Paste", "CTRL+V"),
                }),
                new UIMenu("GameObject", new IUIComponent[]
                {
                    new UIMenu("Create", new IUIComponent[]
                    {
                        new UIMenuItem("3D Object", readOnly: true),
                        new UIMenuItem("Cube"),
                        new UIMenuItem("Sphere"),
                        new UIMenuItem("Cylender"),
                        new UIMenuItem("Plane"),
                        new UIMenuItem("3D Text"),

                        new UIMenuItem("2D Object", readOnly: true),
                        new UIMenuItem("Comming Soon", readOnly: true),
                    }),
                }),


                new UIMenu("Windows", new IUIComponent[]
                {
                    new UIMenuItem("Scene", action: () =>
                    {
                        if(uihost.Children.Find(x => x.Name == "Scene") == null)
                            uihost.Children.Add(UIUtils.CreateWindow(new WindowConfig()
                            {
                                name = "Scene",
                                device = gd,
                                view = imguiView,
                                color = new RgbaFloat(1f, 1f, 1f, 1f),
                            }));
                        else
                           ((UIWindow)uihost.Children.Find(x => x.Name == "Scene")).Open = true;
                    }),
                    new UIMenuItem("Game", action: () =>
                    {
                        if(uihost.Children.Find(x => x.Name == "Game") == null)
                            uihost.Children.Add(UIUtils.CreateWindow(new WindowConfig()
                            {
                                name = "Game",
                                device = gd,
                                view = imguiView,
                                components = new IUIComponent[]
                                {
                                    new UIMenuBar(new IUIComponent[]
                                    {
                                        new UIMenu("Status", new IUIComponent[]
                                        {
                                            new UIMenuItem($"FPS: {ImGui.GetIO().Framerate}" ),
                                        }),
                                    }),
                                }
                            }));
                         else
                           ((UIWindow)uihost.Children.Find(x => x.Name == "Game")).Open = true;
                    }),
                    new UIMenuItem("Hierarchy", action: () =>
                    {
                        if(uihost.Children.Find(x => x.Name == "Hierarchy") == null)
                            uihost.Children.Add(UIUtils.CreateWindow(new WindowConfig()
                            {
                                name = "Hierarchy",
                                device = gd,
                                view = imguiView,
                                components = new IUIComponent[]
                                {

                                }
                            }));
                        else
                           ((UIWindow)uihost.Children.Find(x => x.Name == "Hierarchy")).Open = true;
                    }),
                    new UIMenuItem("Inspector", action: () =>
                    {
                        if(uihost.Children.Find(x => x.Name == "Inspector") == null)
                            uihost.Children.Add(UIUtils.CreateWindow(new WindowConfig()
                            {
                                name = "Inspector",
                                device = gd,
                                view = imguiView,
                                components = new IUIComponent[]
                                {
                                    new ComponentHeader(),
                                }
                            }));
                        else
                           ((UIWindow)uihost.Children.Find(x => x.Name == "Inspector")).Open = true;
                    }),
                    new UIMenuItem("Project", action: () =>
                    {
                        if(uihost.Children.Find(x => x.Name == "Project") == null)
                            uihost.Children.Add(UIUtils.CreateWindow(new WindowConfig()
                            {
                                name = "Project",
                                device = gd,
                                view = imguiView,
                                components = new IUIComponent[]
                                {

                                }
                            }));
                        else
                           ((UIWindow)uihost.Children.Find(x => x.Name == "Project")).Open = true;
                    }),
                    new UIMenuItem("Console", action: () =>
                    {
                        if(uihost.Children.Find(x => x.Name == "Console") == null)
                            uihost.Children.Add(UIUtils.CreateWindow(new WindowConfig()
                            {
                                name = "Console",
                                device = gd,
                                view = imguiView,
                                components = new IUIComponent[]
                                {

                                }
                            }));
                        else
                           ((UIWindow)uihost.Children.Find(x => x.Name == "Console")).Open = true;
                    }),
                })
            });


           
            var sceneWindow = UIUtils.CreateWindow(new WindowConfig()
            {
                name = "Scene",
                device = gd,
                view = imguiView,
                color = new RgbaFloat(.26f, .24f, .10f, 1f),
            });            
            var gameWindow = UIUtils.CreateWindow(new WindowConfig()
            {
                name = "Game",
                device = gd,
                view = imguiView,
                components = new IUIComponent[]
                {
                    new UIMenuBar(new IUIComponent[]
                    {
                        new UIMenu("Status", new IUIComponent[]
                        {
                            new UIMenuItem($"FPS: {ImGui.GetIO().Framerate}" ),
                        }),
                    }),
                }
            });
            var hierarchyWindow = UIUtils.CreateWindow(new WindowConfig()
            {
                name = "Hierarchy",
                device = gd,
                view = imguiView,
                components = new IUIComponent[]
                {

                }
            });
            var inspectorWindow = UIUtils.CreateWindow(new WindowConfig()
            {
                name = "Inspector",
                device = gd,
                view = imguiView,
                components = new IUIComponent[]
                {
                    new ComponentHeader(),
                }
            });
            var projectWindow = UIUtils.CreateWindow(new WindowConfig()
            {
                name = "Project",
                device = gd,
                view = imguiView,
                components = new IUIComponent[]
                {

                }
            });
            var consoleWindow = UIUtils.CreateWindow(new WindowConfig()
            {
                name = "Console",
                device = gd,
                view = imguiView,
                components = new IUIComponent[]
                {

                }
            });

            var pattern = new MIDIPattern();
            projectConnect = new ProjectConnect();
            var pianoRollWindow = UIUtils.CreatePianoRollWindow(projectConnect, pattern, gd, imguiView);

            // Initialize imgui UI
            uihost = disposer.Add(new UIHost(new IUIComponent[] 
            {
                mainmenu, 
                sceneWindow, 
                //gameWindow, 
                hierarchyWindow, 
                inspectorWindow,
                //projectWindow,
                //consoleWindow,
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
