using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace YGR
{
    public class Y_Camera
    {
        public float Zoom { get; set; }
        public Vector2 Position { get; set; }
        public Rectangle Bounds { get; protected set; }
        public Rectangle VisibleArea { get; protected set; }
        public Matrix Transform { get; protected set; }

        private const float minZoom = .25f;
        private const float maxZoom = 2f;
        private const float zoomSpeed = 0.05f;
        private const float panSpeed = 1024;

        private float currentMouseWheelValue, previousMouseWheelValue;

        public Y_Camera(Viewport viewport, Vector2 position)
        {
            Bounds = viewport.Bounds;
            Zoom = 1f;
            Position = position;
        }

        private void UpdateVisibleArea()
        {
            var inverseViewMatrix = Matrix.Invert(Transform);

            var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
            var tr = Vector2.Transform(new Vector2(Bounds.X, 0), inverseViewMatrix);
            var bl = Vector2.Transform(new Vector2(0, Bounds.Y), inverseViewMatrix);
            var br = Vector2.Transform(new Vector2(Bounds.Width, Bounds.Height), inverseViewMatrix);

            var min = new Vector2(
                MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
            var max = new Vector2(
                MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
            VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        private void UpdateMatrix()
        {
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
            UpdateVisibleArea();
        }

        public void MoveCamera(Vector2 movePosition)
        {
            Vector2 newPosition = Position + movePosition;
            Position = newPosition;
        }

        public void AdjustZoom(float zoomAmount)
        {
            Zoom += zoomAmount;
            if (Zoom < minZoom) Zoom = minZoom;
            if (Zoom > maxZoom) Zoom = maxZoom;
        }

        public void UpdateCamera(Viewport bounds, float deltaTime)
        {
            Bounds = bounds.Bounds;
            UpdateMatrix();

            Vector2 cameraMovement = Vector2.Zero;
            float moveSpeed = deltaTime * panSpeed / (float)Math.Sqrt(Zoom);

            if (Keyboard.IsPressed(Keybinds.CameraMoveLeft)) cameraMovement.X = -moveSpeed;
            if (Keyboard.IsPressed(Keybinds.CameraMoveRight)) cameraMovement.X = moveSpeed;
            if (Keyboard.IsPressed(Keybinds.CameraMoveUp)) cameraMovement.Y = -moveSpeed;
            if (Keyboard.IsPressed(Keybinds.CameraMoveDown)) cameraMovement.Y = moveSpeed;

            previousMouseWheelValue = currentMouseWheelValue;
            currentMouseWheelValue = Mouse.GetState().ScrollWheelValue;
            AdjustZoom(Math.Clamp(currentMouseWheelValue - previousMouseWheelValue, -1, 1) * zoomSpeed);

            MoveCamera(cameraMovement);
        }
    }
}