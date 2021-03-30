using Herno.Renderer;
using Herno.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;
using System.Numerics;
using ImGuiNET;
using Herno.MIDI;
using Herno.Util;
using Rectangle = Herno.Util.Rectangle;
using Herno.Components.PianoRoll.Interactions;
using System.Threading;
using Herno.Components;

namespace Herno.Components
{
    public class Viewport : UIViewport
    {
        struct VertexPositionColor
        {
            public Vector2 Position; // This is the position, in normalized device coordinates.
            public RgbaFloat Color; // This is the color of the vertex.
            public VertexPositionColor(Vector2 position, RgbaFloat color)
            {
                Position = position;
                Color = color;
            }
            public const uint SizeInBytes = 24;
        }

        BufferList<VertexPositionColor>[] Buffers { get; }
        DeviceBuffer ProjMatrix { get; set; }
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        ResourceLayout Layout { get; set; }
        ResourceSet MainResourceSet { get; set; }
        Shader[] Shaders { get; set; }
        Pipeline Pipeline { get; set; }

        //IPianoRollInteraction CurrentInteraction { get; set; }
        public int[] FirstRenderNote { get; } = new int[Constants.KeyCount];
        public double LastRenderLeft { get; set; } = 0;

        const string VertexCode = @"
      #version 450

      layout(location = 0) in vec2 Position;
      layout(location = 1) in vec4 Color;

      layout (binding = 0) uniform ProjectionMatrixBuffer
      {
          mat4 projection_matrix;
      };

      layout(location = 0) out vec4 fsin_Color;

      void main()
      {
          gl_Position = projection_matrix * vec4(Position, 0, 1);
          fsin_Color = Color;
      }";

        const string FragmentCode = @"
      #version 450

      layout(location = 0) in vec4 fsin_Color;
      layout(location = 0) out vec4 fsout_Color;

      void main()
      {
          fsout_Color = fsin_Color;
      }";

        DisposeGroup dispose = new DisposeGroup();

        public Viewport(GraphicsDevice gd, ImGuiView view, Func<Vector2> computeSize) : base(gd, view, computeSize)
        {
            Buffers = new BufferList<VertexPositionColor>[257]; // first buffer for general purpose, others for keys
            for (int i = 0; i < 257; i++)
                Buffers[i] = dispose.Add(new BufferList<VertexPositionColor>(gd, 6 * 2048 * 16, new[] { 0, 3, 2, 0, 2, 1 }));
            //CurrentInteraction = new MIDIPatternInteractionIdle(null);

            ProjMatrix = Factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _vertexBuffer = Factory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer = Factory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));
            dispose.Add(ProjMatrix);
            dispose.Add(_vertexBuffer);
            dispose.Add(_indexBuffer);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
              new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
              new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            Layout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
            dispose.Add(Layout);

            ShaderDescription vertexShaderDesc = new ShaderDescription(
              ShaderStages.Vertex,
              Encoding.UTF8.GetBytes(VertexCode),
              "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
              ShaderStages.Fragment,
              Encoding.UTF8.GetBytes(FragmentCode),
              "main");

            Shaders = dispose.AddArray(Factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc));

            var pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
              depthTestEnabled: true,
              depthWriteEnabled: true,
              comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
              cullMode: FaceCullMode.Front,
              fillMode: PolygonFillMode.Solid,
              frontFace: FrontFace.Clockwise,
              depthClipEnabled: true,
              scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { Layout };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
              vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
              shaders: Shaders);
            pipelineDescription.Outputs = Canvas.FrameBuffer.OutputDescription;

            Pipeline = Factory.CreateGraphicsPipeline(pipelineDescription);

            MainResourceSet = Factory.CreateResourceSet(new ResourceSetDescription(Layout, ProjMatrix));
        }

        protected override void ProcessInputs()
        {
            base.ProcessInputs();

            /*
            CurrentInteraction.Act();
            while (CurrentInteraction.NextInteraction != null)
            {
                CurrentInteraction = CurrentInteraction.NextInteraction;
            }
            */
        }

        protected override void RenderToCanvas(CommandList cl)
        {
            cl.SetFramebuffer(Canvas.FrameBuffer);
            cl.ClearColorTarget(0, new RgbaFloat(.10f, .10f, .10f, 1f));
            Matrix4x4 mvp = Matrix4x4.Identity * Matrix4x4.CreateScale(2, 2, 1) * Matrix4x4.CreateScale(1.0f / this.Canvas.Width, 1.0f / this.Canvas.Height, 1) * Matrix4x4.CreateTranslation(-1, -1, 0) * Matrix4x4.CreateScale(1, -1, 1);
            GraphicsDevice.UpdateBuffer(ProjMatrix, 0, ref mvp);
           
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            
            cl.SetPipeline(Pipeline);
            cl.SetGraphicsResourceSet(0, MainResourceSet);

            cl.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            GraphicsDevice.SubmitCommands(cl);
            GraphicsDevice.SwapBuffers();

            foreach (var x in Buffers) x.Reset();


        

            //var viewFrame = Handler.ViewFrame;

            var canvasSize = ImGui.GetItemRectSize();

        
            bool QuadVisible(Rectangle rect)
            {
                return rect.Top < canvasSize.Y && rect.Bottom > 0 && rect.Left < canvasSize.X && rect.Right > 0;
            }

            void PushQuad(int bufferIdx, Rectangle rect, RgbaFloat col)
            {
                if (!QuadVisible(rect))
                    return;
                float top = (float)rect.Top, bottom = (float)rect.Bottom, left = (float)rect.Left, right = (float)rect.Right;
                var buffer = Buffers[bufferIdx];
                buffer.Push(cl, new VertexPositionColor(new Vector2(left, top), col));
                buffer.Push(cl, new VertexPositionColor(new Vector2(right, top), col));
                buffer.Push(cl, new VertexPositionColor(new Vector2(right, bottom), col));
                buffer.Push(cl, new VertexPositionColor(new Vector2(left, bottom), col));
            }

           

            foreach (var x in Buffers) x.Flush(cl);

            ImGui.SetCursorPos(ImGui.GetCursorStartPos());
        }
    }
}