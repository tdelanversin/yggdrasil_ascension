using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace YGR
{
    public enum CameraMode
    {
        Follow = 0, // Follow players
        Room,       // Focus on a room
        Menu,       // Focus on menu
        Manual      // Control pos and zoom with keybinds
    }
    public static class Camera
    {
        public static float Zoom { get; set; }
        public static Vector2 Position { get; set; }
        public static Rectangle Bounds { get; set; }
        public static Rectangle VisibleArea { get; private set; }
        public static Matrix Transform { get; private set; }
        public static CameraMode Mode { get; private set; }
        public static CameraMode ModePrev { get; private set; }
        public static IWalkable Room { get; private set; } // Room to focus on
        public static Rectangle Rect { get; private set; } // Rect to focus on

        // Animation related fields useful for other classes to decide when and what to draw
        public static float AnimationFraction { get; private set; }
        public static bool InAnimation { get; private set; }
        public static bool InTransitionToMenu { get; private set; }
        public static bool InTransitionFromMenu { get; set; }

        private static float _animationDuration = 1000;
        private static float _animationTimer = _animationDuration;
        private static float _transitionalZoom;

        private static Vector2 _transitionalPosition;
        private static float _previousZoom;
        private static Vector2 _previousPosition;


        // Zoom levels for...                     { Follow, Room, Rect, Manual }
        private static readonly float[] minZoom = { 0.05f, 0.15f, 0.05f, 0.05f };
        private static readonly float[] maxZoom = { 4.00f, 4.00f, 16.0f, 16.0f };
        private const float zoomSpeed = 0.1f;
        private const float panSpeed = 1;

        private static float currentMouseWheelValue, previousMouseWheelValue;

        public static void Initialize(GraphicsDeviceManager gdm, CameraMode mode)
        {
            var res_x = gdm.PreferredBackBufferWidth;
            var res_y = gdm.PreferredBackBufferHeight;
            Position = new Vector2(res_x / 2, res_y / 2);
            _transitionalPosition = Position;
            Zoom = 1f;
            _transitionalZoom = Zoom;
            Bounds = gdm.GraphicsDevice.Viewport.Bounds;
            Mode = mode;
        }

        private static void UpdateVisibleArea()
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

        private static void UpdateMatrix()
        {
            Transform = Matrix.CreateTranslation(new Vector3(-_transitionalPosition.X, -_transitionalPosition.Y, 0)) *
                    Matrix.CreateScale(_transitionalZoom) *
                    Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
            UpdateVisibleArea();
        }

        private static void MoveCamera(Vector2 movePosition)
        {
            Vector2 newPosition = Position + movePosition;
            Position = newPosition;
        }

        private static void UpdateZoom(float zoom)
        {
            // Clamp to the min/max zoom level allowed in the current mode
            Zoom = Math.Clamp(zoom, minZoom[(int)Mode], maxZoom[(int)Mode]);
        }

        private static void keyboardMove(GameTime gameTime)
        {
            Vector2 cameraMovement = Vector2.Zero;
            float moveSpeed = gameTime.ElapsedGameTime.Milliseconds * panSpeed / (float)Math.Sqrt(Zoom);

            if (Input.IsKeyDown(Keybinds.CameraMoveLeft)) cameraMovement.X = -moveSpeed;
            if (Input.IsKeyDown(Keybinds.CameraMoveRight)) cameraMovement.X = moveSpeed;
            if (Input.IsKeyDown(Keybinds.CameraMoveUp)) cameraMovement.Y = -moveSpeed;
            if (Input.IsKeyDown(Keybinds.CameraMoveDown)) cameraMovement.Y = moveSpeed;
            MoveCamera(cameraMovement);

            previousMouseWheelValue = currentMouseWheelValue;
            currentMouseWheelValue = Mouse.GetState().ScrollWheelValue;
            if (currentMouseWheelValue > previousMouseWheelValue)
            {
                UpdateZoom(Zoom * (1 + zoomSpeed));
            }
            else if (currentMouseWheelValue < previousMouseWheelValue)
            {
                UpdateZoom(Zoom / (1 + zoomSpeed));
            }
        }

        private static void centerOnPlayers()
        {
            if (Manager_Players.Players == null || Manager_Players.Players.Count < 1) return;

            var left = Manager_Players.Players[0].Rect.X;
            var right = Manager_Players.Players[0].Rect.X;
            var top = Manager_Players.Players[0].Rect.Y;
            var bot = Manager_Players.Players[0].Rect.Y;

            Vector2 playerMeanPos = Vector2.Zero;
            foreach (var player in Manager_Players.Players)
            {
                playerMeanPos += player.Rect.Location.ToVector2();
                left = Math.Min(player.Rect.X, left);
                right = Math.Max(player.Rect.X + player.Rect.Width, right);
                top = Math.Min(player.Rect.Y, top);
                bot = Math.Max(player.Rect.Y + player.Rect.Height, bot);
            }

            // Update camera position
            playerMeanPos /= Manager_Players.Players.Count;
            Position = playerMeanPos;
            // Console.WriteLine(playerMeanPos);

            // Set zoom level to fit all players
            var boundsStretchFactor = Math.Max((right - left) / (float)Bounds.Width, (bot - top) / (float)Bounds.Height);
            var resolutionAdjustment = Bounds.Width / 1920f;
            var overStretch = 0.65f;
            var zoomMarginFactor = Math.Min(overStretch, overStretch * resolutionAdjustment);
            var zoomFactor = zoomMarginFactor / boundsStretchFactor;
            var zoomFactorMax = 0.95f * resolutionAdjustment;
            var zoom = Math.Min(zoomFactor, zoomFactorMax);
            UpdateZoom(zoom);
        }

        private static void focusOnRoom()
        {
            if (Room == null)
            {
                Logger.Error("CameraMode set to Room but Room is not defined.");
                return;
            }
            Position = new Vector2(Room.Rect.X + Room.Rect.Width / 2, Room.Rect.Y + Room.Rect.Height / 2);
            var stretch = Math.Max((float)Room.Rect.Width / Bounds.Width, (float)Room.Rect.Height / Bounds.Height);
            UpdateZoom(.95f / stretch); // Show a bit more than just the room
        }

        private static void focusOnMenu()
        {
            Position = new Vector2(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2);

            // Actually take the Min here. Unlike with room focus, the goal is
            // not to show the entire Title Image no matter the price, but
            // instead we prefer not to have ugly borders on a non-16:9 aspect ratio.
            var stretch = Math.Min((float)Rect.Width / Bounds.Width, (float)Rect.Height / Bounds.Height);
            UpdateZoom(1.0f / stretch);
        }

        public static void CycleCameraMode()
        {
            switch (Mode)
            {
                case CameraMode.Manual:
                    SetFocusPlayers();
                    break;
                case CameraMode.Follow:
                    SetFocusRoom(Room);
                    break;
                case CameraMode.Room:
                    SetFocusManual();
                    break;
                default:
                    break;
            }
        }

        private static double SigmoidCentered(double t, double a)
        {
            return (1d / (1 + Math.Exp(-a * t)) - 0.5d);
        }

        public static double EaseInOut(double t, double k)
        {
            return (0.5d / SigmoidCentered(1, k)) * SigmoidCentered(2 * t - 1, k) + 0.5d;
        }

        private static void UpdateAnimation(GameTime gameTime)
        {

            if (_animationTimer < _animationDuration)
            {

                // var animationFraction = _animationTimer / _animationDuration; // linear

                // Hand-made Slow-in, slow-out function, using sin(): (1+sin(pi*x/500-pi/2))/2
                // https://www.wolframalpha.com/input?i=plot+%281%2Bsin%28pi*x%2F1-pi%2F2%29%29%2F2+from+-0.2+to+1.2
                // float animationFraction = (float)(1d + Math.Sin(Math.PI * _animationTimer / _animationDuration - Math.PI / 2d)) / 2f;

                // EaseInOut using Sigmoid and a high steepnes factor
                // https://www.wolframalpha.com/input?i=plot+1+%2F+%281+%2B+exp%28-12%28x+-+1%2F2%29%29%29+from+-0.2+to+1.2
                // float animationFraction = (float)(1f / (1 + Math.Exp(-16 * (_animationTimer / _animationDuration - 0.5d))));

                // Even better version that works for low factors of k too
                // https://www.wolframalpha.com/input?i=plot+%28%280.5+%2F+%281%2F%281%2Bexp%28-k%29%29-0.5%29%29*%281%2F%281%2Bexp%28-k*%282x-1%29%29%29-0.5%29%2B0.5%29+x+from+-0.2+to+1.2%2C+k+%3D+8
                AnimationFraction = (float)EaseInOut(t: _animationTimer / _animationDuration, k: 6d);

                _transitionalPosition = (Position * AnimationFraction) + _previousPosition * (1f - AnimationFraction);
                _transitionalZoom = (Zoom * AnimationFraction) + _previousZoom * (1f - AnimationFraction);

                _animationTimer += gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                _transitionalPosition = Position;
                _transitionalZoom = Zoom;
                InAnimation = false;
                InTransitionFromMenu = false;
                InTransitionToMenu = false;
            }
        }

        public static void Update(Viewport bounds, GameTime gameTime)
        {
            Bounds = bounds.Bounds;
            switch (Mode)
            {
                case CameraMode.Manual:
                    keyboardMove(gameTime);
                    break;

                case CameraMode.Follow:
                    centerOnPlayers();
                    break;

                case CameraMode.Room:
                    focusOnRoom();
                    break;

                case CameraMode.Menu:
                    focusOnMenu();
                    break;
            }
            UpdateAnimation(gameTime);
            UpdateMatrix();
        }

        private static void ResetAnimation(float animationDuration)
        {
            _animationDuration = animationDuration;
            _previousPosition = _transitionalPosition;
            _previousZoom = _transitionalZoom;
            _animationTimer = 0;
            InAnimation = true;
        }

        public static void ToggleManualMode()
        {
            if (Mode != CameraMode.Manual)
                SetFocusManual();
            else
                Mode = ModePrev;
        }

        public static void RestorePreviousMode(bool animate = true, float animationDuration = 1000)
        {
            if (Mode != CameraMode.Menu)
            {
                return;
            }
            InTransitionFromMenu = true;

            if (animate)
            {
                ResetAnimation(animationDuration);
            }
            var tmp = Mode;
            Mode = ModePrev;
            ModePrev = tmp;
        }

        public static void SetFocusManual()
        {
            Mode = CameraMode.Manual;
        }

        /// <summary> Center the camera on position. </summary>
        public static void SetFocusManual(Vector2 position, float zoom, bool animate = true, float animationDuration = 1000)
        {
            if (animate)
            {
                ResetAnimation(animationDuration);
            }
            Position = position;
            UpdateZoom(zoom);
            Mode = CameraMode.Manual;
        }

        public static void SetFocusPlayers(bool animate = true, float animationDuration = 1000)
        {
            if (animate)
            {
                ResetAnimation(animationDuration);
            }
            Mode = CameraMode.Follow;
        }

        public static void SetFocusRoom(IWalkable room, bool animate = true, float animationDuration = 1000)
        {
            ModePrev = Mode;
            if (animate)
            {
                ResetAnimation(animationDuration);
            }
            Room = room;
            Mode = CameraMode.Room;
        }

        public static void SetFocusMenu(Rectangle rect, bool animate = true, float animationDuration = 1000)
        {
            ModePrev = Mode;
            if (animate)
            {
                ResetAnimation(animationDuration);
            }
            InTransitionToMenu = true;
            Rect = rect;
            Mode = CameraMode.Menu;
        }
    }
}