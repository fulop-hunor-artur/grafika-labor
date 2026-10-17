using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;

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

        public static float a = (float)Math.Tan(Math.PI / 6) * 4 - 2;
        public static Vector3 A = new(0, 0, 0);
        public static Vector3 B = new(-4, 2 + a, 0);
        public static Vector3 C = new(4, 2 + a, 0);
        public static Vector3 D = new(-4, 7, 0);
        public static Vector3 E = new(4, 7, 0);
        public static Vector3 F = new(0, 4 + a, 0);
        public static Vector3 G = new(0, 9 + a, 0);

        public static Vector3 RubicksMid(Vector3 a, Vector3 b) {
            return a / 3 + b * 2 / 3;
        }

        public static Vector3[] TRINAGLE_VERTECES = new Vector3[] {
            A, A, // (alpha, beta)
            B, // (alpha)
            C, // (beta)
            D, D, // (alpha, omega)
            E, E, // (beta, omega)
            F, F, F, // (alpha, beta, omega)
            G // (omega)
        };

        public static Vector3[] LINE_VERTECES = new Vector3[] {
            A, B,
            A, C,
            A, F,
            B, D,
            C, E,
            D, F,
            D, G,
            E, F,
            E, G,
            RubicksMid(A, B), RubicksMid(F, D),
            RubicksMid(B, A), RubicksMid(D, F),
            RubicksMid(A, F), RubicksMid(B, D),
            RubicksMid(F, A), RubicksMid(D, B),
            RubicksMid(A, C), RubicksMid(F, E),
            RubicksMid(C, A), RubicksMid(E, F),
            RubicksMid(A, F), RubicksMid(C, E),
            RubicksMid(F, A), RubicksMid(E, C),
            RubicksMid(F, E), RubicksMid(D, G),
            RubicksMid(E, F), RubicksMid(G, D),
            RubicksMid(F, D), RubicksMid(E, G),
            RubicksMid(D, F), RubicksMid(G, E),
        };

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

        private static float[] Flatten(Vector3[] vectors) {
            return vectors.SelectMany<Vector3, float>(v => [v.X, v.Y, v.Z]).ToArray();
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            float xOff = 0f;
            float yOff = -4.7f;
            float scale = 0.2f;
            float a = (float)Math.Tan(Math.PI / 6) * 4 - 2;

            float[] vertexArray = Flatten(TRINAGLE_VERTECES);
            OffsetAndScale(vertexArray, xOff, yOff, scale);

            float[] pallette = new float[] {
                1.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f,
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

            float[] lineVertexArray = Flatten(LINE_VERTECES);
            OffsetAndScale(lineVertexArray, xOff, yOff, scale);

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

            uint lineVertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineVertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)lineVertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);

            // Instead of big array, I can disable the attribute array and tell it to use a constant value for it!
            Gl.DisableVertexAttribArray(1);
            Gl.VertexAttrib4(1, 0.0f, 0.0f, 0.0f, 1.0f);

            Gl.LineWidth(3.0f);
            Gl.DrawArrays(GLEnum.Lines, 0, (uint)(lineVertexArray.Length / 3));

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(lineVertices);
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}
