using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. labor";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static void OffsetAndScale(float[] vertexArray, float offsetX, float offsetY, float scale) {
            for (int i = 0; i < vertexArray.Count(); i += 3) {
                vertexArray[i+0] = (vertexArray[i+0] + offsetX) * scale;
                vertexArray[i+1] = (vertexArray[i+1] + offsetY) * scale;
            }
        }

        private static float[] GetColorArray(float[] pallette, int[] sequence) {
            const int STRIDE = 4;
            float[] colorArray = new float[sequence.Length * STRIDE];
            for (int i = 0; i < sequence.Length; i++) {
                colorArray[i * STRIDE + 0] = pallette[sequence[i] * STRIDE + 0];
                colorArray[i * STRIDE + 1] = pallette[sequence[i] * STRIDE + 1];
                colorArray[i * STRIDE + 2] = pallette[sequence[i] * STRIDE + 2];
                colorArray[i * STRIDE + 3] = pallette[sequence[i] * STRIDE + 3];
            }
            return colorArray;
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            float xOff = 0f;
            float yOff = -4.5f;
            float scale = 0.1f;
            float a = (float)Math.Tan(Math.PI / 6) * 4;

            float[] vertexArray = new float[] {
                 0, 0, 0, // A (alpha)
                 0, 0, 0, // A (beta)
                -4, 2 + a, 0, // B (alpha)
                 4, 2 + a, 0, // C (beta)
                -4, 7, 0, // D (alpha)
                -4, 7, 0, // D (omega)
                 4, 7, 0, // E (beta)
                 4, 7, 0, // E (omega)
                 0, 4 + a, 0, // F (alpha)
                 0, 4 + a, 0, // F (beta)
                 0, 4 + a, 0, // F (omega)
                 0, 9 + a, 0 // G (omega)
            };
            OffsetAndScale(vertexArray, xOff, yOff, scale);

            float[] pallette = new float[] {
                1.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 0.1f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f
            };

            int[] colorSequence = new int[] {
                0, 1, 0, 1, 0, 2, 1, 2, 0, 1, 2, 2
            };

            float[] colorArray = GetColorArray(pallette, colorSequence);

            uint[] indexArray = new uint[] { 
                2, 0, 8,
                8, 4, 2,
                1, 3, 6,
                6, 9, 1,
                10, 7, 5,
                7, 11, 5
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);
            
            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}
