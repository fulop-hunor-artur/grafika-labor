
using Silk.NET.Maths;

namespace Szeminarium
{
    internal class CameraDescriptor
    {
        private const float MaxPitch = MathF.PI / 2f - 0.01f;
        public Vector3D<float> Position { get; private set; } = new(2, 2, 2);

        public float Yaw { get; private set; } = MathF.PI / 4f;
        public float Pitch { get; private set; } = -MathF.Asin(1f / MathF.Sqrt(3f));
        public float Roll { get; private set; } = 0;

        public Vector3D<float> Up = Vector3D<float>.UnitY;

        public Vector3D<float> Target {
            get { 
                return Position + Forward;
            }
        }

        public Vector3D<float> Forward {
            get {
                return Vector3D.Transform(
                    -Vector3D<float>.UnitZ,
                    Matrix4X4.CreateFromYawPitchRoll(Yaw, Pitch, Roll)
                );
            }
        }

        public void ChangeYaw(float mult) {
            Yaw += mult;
        }

        public void ChangePitch(float mult) { 
            Pitch = Math.Clamp(Pitch + mult, -MaxPitch, MaxPitch);
        }

        public void ChangeLocalY(float mult) {
            Position = Position + new Vector3D<float>(0f, mult, 0f);
        }

        public void ChangeLocalXZ(float mult) {
            var forwardXZ = Vector3D.Normalize(new Vector3D<float>(Forward.X, 0f, Forward.Z));
            Position = Position + forwardXZ * mult;
        }

        public void ChangeLocalXZStrafe(float mult) {
            var rightXZ = Vector3D.Normalize(Vector3D.Cross(Forward, Up));
            Position = Position + rightXZ * mult;
        }
    }
}
