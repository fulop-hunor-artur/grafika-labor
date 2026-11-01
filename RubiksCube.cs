using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Drawing;

namespace GrafikaSzeminarium
{
    internal class RubiksCube : IDisposable
    {
        private readonly GL gl;
        private ModelObjectDescriptor[] rubicksCubes = null!;

        private float rubicksAngle = 0;
        private float targetRubicksAngle = 0;

        public RubiksCube(GL gl)
        {
            this.gl = gl;
            CreateMeshes();
        }

        public void CreateMeshes()
        {
            rubicksCubes = new ModelObjectDescriptor[27];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Color c1 = j == 2 ? Color.Red : Color.Black;
                        Color c2 = k == 2 ? Color.Blue : Color.Black;
                        Color c3 = i == 0 ? Color.Green : Color.Black;
                        Color c4 = j == 0 ? Color.Violet : Color.Black;
                        Color c5 = k == 0 ? Color.Yellow : Color.Black;
                        Color c6 = i == 2 ? Color.LightBlue : Color.Black;
                        rubicksCubes[i * 9 + j * 3 + k] = ModelObjectDescriptor.CreateCube(gl, c1, c2, c3, c4, c5, c6);
                    }
                }
            }
        }

        public void Rotate(float angle)
        {
            targetRubicksAngle += angle;
        }

        public void Update(double deltaTime)
        {
            if (targetRubicksAngle > rubicksAngle)
            {
                rubicksAngle = (float)Math.Min(rubicksAngle + 0.05, targetRubicksAngle);
            }
            else if (targetRubicksAngle < rubicksAngle)
            {
                rubicksAngle = (float)Math.Max(rubicksAngle - 0.05, targetRubicksAngle);
            }
        }

        public unsafe void Render(Action<Matrix4X4<float>> setModelMatrix)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        var scale = Matrix4X4.CreateScale(0.5f);
                        var trans1 = Matrix4X4.CreateTranslation(0, 0, (k - 1) * 0.55f);
                        var rot = Matrix4X4.CreateRotationZ(k == 0 ? rubicksAngle : 0);
                        var trans2 = Matrix4X4.CreateTranslation((i - 1) * 0.55f, (j - 1) * 0.55f, 0);
                        setModelMatrix(scale * trans2 * rot * trans1);
                        DrawModelObject(rubicksCubes[i * 9 + j * 3 + k]);
                    }
                }
            }
        }

        private unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            gl.BindVertexArray(modelObject.Vao);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            foreach (var cube in rubicksCubes)
                cube.Dispose();
        }
    }
}
