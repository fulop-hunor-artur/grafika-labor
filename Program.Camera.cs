using Silk.NET.Input;
using System.Numerics;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal partial class Program
    {
        private static CameraDescriptor camera = new CameraDescriptor();
        private static bool cameraFrozen = false;
        private static Vector2? lastMousePosition = null;
        private const float MouseSensitivity = 0.002f;

        private static void InitCameraInput()
        {
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Camera_KeyDown;
            }
            foreach (var mouse in inputContext.Mice)
            {
                mouse.Cursor.CursorMode = CursorMode.Disabled;
                mouse.MouseMove += Mouse_MouseMove;
            }
        }

        private static void Camera_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                ToggleCameraFreeze();
            }
        }

        private static void Mouse_MouseMove(IMouse mouse, Vector2 position)
        {
            if (cameraFrozen)
                return;

            if (lastMousePosition == null)
            {
                lastMousePosition = position;
                return;
            }

            float deltaX = position.X - lastMousePosition.Value.X;
            float deltaY = position.Y - lastMousePosition.Value.Y;
            lastMousePosition = position;

            camera.ChangeYaw(-deltaX * MouseSensitivity);
            camera.ChangePitch(-deltaY * MouseSensitivity);
        }

        private static void ToggleCameraFreeze()
        {
            cameraFrozen = !cameraFrozen;
            lastMousePosition = null;
            foreach (var mouse in inputContext.Mice)
            {
                mouse.Cursor.CursorMode = cameraFrozen ? CursorMode.Normal : CursorMode.Disabled;
            }
        }

        private static void NewCameraControls(double deltaTime)
        {
            if (cameraFrozen)
                return;

            float MOVEMENT_SPEED = 2.5f;
            foreach (var keyboard in inputContext.Keyboards)
            {
                if (keyboard.IsKeyPressed(Key.W))
                {
                    camera.ChangeLocalXZ((float)deltaTime * MOVEMENT_SPEED);
                }
                if (keyboard.IsKeyPressed(Key.S))
                {
                    camera.ChangeLocalXZ((float)-deltaTime * MOVEMENT_SPEED);
                }
                if (keyboard.IsKeyPressed(Key.A))
                {
                    camera.ChangeLocalXZStrafe((float)-deltaTime * MOVEMENT_SPEED);
                }
                if (keyboard.IsKeyPressed(Key.D))
                {
                    camera.ChangeLocalXZStrafe((float)deltaTime * MOVEMENT_SPEED);
                }
                if (keyboard.IsKeyPressed(Key.Space))
                {
                    camera.ChangeLocalY((float)deltaTime * MOVEMENT_SPEED);
                }
                if (keyboard.IsKeyPressed(Key.ShiftLeft))
                {
                    camera.ChangeLocalY((float)-deltaTime * MOVEMENT_SPEED);
                }
            }
        }
    }
}
