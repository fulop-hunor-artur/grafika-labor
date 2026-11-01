using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Drawing;

namespace GrafikaSzeminarium
{
    internal class RubiksCube : IDisposable
    {
        private readonly GL gl;
        private ModelObjectDescriptor[] cubeModels = null!;
        private Matrix4X4<float>[] cubePersonalRotations = null!;

        private bool animating = false;
        private float animTimer = 0;
        private const float MAX_ANIM_TIMER = 0.75f;
        private Axis animAxis = Axis.X;
        private int animAxisIndex = 0;
        private int animRotationMultiplier = 1; // 1 or -1

        public RubiksCube(GL gl)
        {
            this.gl = gl;
            CreateMeshes();
        }

        public void CreateMeshes()
        {
            cubeModels = new ModelObjectDescriptor[27];
            cubePersonalRotations = new Matrix4X4<float>[27];
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
                        cubeModels[i * 9 + j * 3 + k] = ModelObjectDescriptor.CreateCube(gl, c1, c2, c3, c4, c5, c6);
                        cubePersonalRotations[i * 9 + j * 3 + k] = Matrix4X4<float>.Identity;
                    }
                }
            }
        }

        public void Rotate(Axis axis, int stripIndex, bool backwards = false)
        {
            if (animating || stripIndex < 0 || stripIndex > 2)
                return;

            animating = true;
            animAxis = axis;
            animAxisIndex = stripIndex;
            animTimer = 0;
            animRotationMultiplier = backwards ? -1 : 1;
        }

        public void RotateEnd()
        {
            animTimer = 0;
            animating = false;

            Matrix4X4<float> stepRot = CreateAxisRotation(animAxis, animRotationMultiplier * (MathF.PI / 2f));

            var nextModels = (ModelObjectDescriptor[])cubeModels.Clone();
            var nextRotations = (Matrix4X4<float>[])cubePersonalRotations.Clone();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (!IsInActiveStrip(i, j, k))
                            continue;

                        var oldPos = new Vector3D<float>(i - 1, j - 1, k - 1);
                        var newPos = Vector3D.Transform(oldPos, stepRot);

                        int newI = (int)MathF.Round(newPos.X) + 1;
                        int newJ = (int)MathF.Round(newPos.Y) + 1;
                        int newK = (int)MathF.Round(newPos.Z) + 1;

                        int oldIdx = i * 9 + j * 3 + k;
                        int newIdx = newI * 9 + newJ * 3 + newK;

                        nextModels[newIdx] = cubeModels[oldIdx];
                        nextRotations[newIdx] = SnapRotation(cubePersonalRotations[oldIdx] * stepRot);
                    }
                }
            }

            cubeModels = nextModels;
            cubePersonalRotations = nextRotations;
        }

        private bool IsInActiveStrip(int i, int j, int k)
        {
            return animAxis switch
            {
                Axis.X => i == animAxisIndex,
                Axis.Y => j == animAxisIndex,
                Axis.Z => k == animAxisIndex,
                _ => false
            };
        }

        private static Matrix4X4<float> CreateAxisRotation(Axis axis, float angle)
        {
            return axis switch
            {
                Axis.X => Matrix4X4.CreateRotationX(angle),
                Axis.Y => Matrix4X4.CreateRotationY(angle),
                Axis.Z => Matrix4X4.CreateRotationZ(angle),
                _ => Matrix4X4<float>.Identity
            };
        }

        private static Matrix4X4<float> SnapRotation(Matrix4X4<float> m)
        {
            return new Matrix4X4<float>(
                (int)MathF.Round(m.M11), (int)MathF.Round(m.M12), (int)MathF.Round(m.M13), (int)MathF.Round(m.M14),
                (int)MathF.Round(m.M21), (int)MathF.Round(m.M22), (int)MathF.Round(m.M23), (int)MathF.Round(m.M24),
                (int)MathF.Round(m.M31), (int)MathF.Round(m.M32), (int)MathF.Round(m.M33), (int)MathF.Round(m.M34),
                (int)MathF.Round(m.M41), (int)MathF.Round(m.M42), (int)MathF.Round(m.M43), (int)MathF.Round(m.M44)
            );
        }

        public void Update(double deltaTime)
        {
            if (animating)
            {
                animTimer += (float)deltaTime;
                if (animTimer >= MAX_ANIM_TIMER)
                {
                    RotateEnd();
                }
            }
        }

        public unsafe void Render(Action<Matrix4X4<float>> setModelMatrix)
        {
            float currentAngle = animating
                ? (animTimer / MAX_ANIM_TIMER) * (MathF.PI / 2f) * animRotationMultiplier
                : 0f;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        int idx = i * 9 + j * 3 + k;
                        var scale = Matrix4X4.CreateScale(0.5f);
                        var personalRot = cubePersonalRotations[idx];
                        var pos = Matrix4X4.CreateTranslation((i - 1) * 0.55f, (j - 1) * 0.55f, (k - 1) * 0.55f);
                        var sliceRot = (animating && IsInActiveStrip(i, j, k))
                            ? CreateAxisRotation(animAxis, currentAngle)
                            : Matrix4X4<float>.Identity;

                        setModelMatrix(scale * personalRot * pos * sliceRot);
                        DrawModelObject(cubeModels[idx]);
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
            foreach (var cube in cubeModels)
                cube.Dispose();
        }
    }

    public enum Axis
    {
        X, Y, Z
    }
}
