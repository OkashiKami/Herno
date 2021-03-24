﻿using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Drawing;
using System.Reflection;
using D3D11 = SharpDX.Direct3D11;

namespace HernoEditor
{
    class EditorWindow : IDisposable
    {
        private RenderForm renderForm;
        private const int Width = 1280;
        private const int Height = 720;

        private D3D11.Device d3dDevice;
        private D3D11.DeviceContext d3dDeviceContext;
        private SwapChain swapChain;
        private D3D11.RenderTargetView renderTargetView;

        /// <summary>
		/// Create and initialize a new game.
		/// </summary>
        public EditorWindow()
        {
            renderForm = new RenderForm("Herno Editor™");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            renderForm.AllowUserResizing = false;
            
            InitializeDeviceResources();
        }

        /// <summary>
		/// Start the game.
		/// </summary>
        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }


        private void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            // Descriptor for the swap chain
            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };

            // Create device and swap chain
            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;

            // Create render target view for back buffer
            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
            {
                renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            }
        }

        /// <summary>
		/// Draw the game.
		/// </summary>
		private void Draw()
        {
            // Set back buffer as current render target view
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);

            // Clear the screen
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(40, 40, 40));

            // Swap front and back buffer
            swapChain.Present(1, PresentFlags.None);
        }

        public void Dispose()
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
        }
    }
} 
