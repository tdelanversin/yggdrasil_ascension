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
        Manual      // Control pos and zoom with keybinds
    }
    public static class Camera
    {
        public static float Zoom { get; set; }
        public static Vector2 Position { get; set; }
        public static Rectangle Bounds { get; set; }
        public static Rectangle VisibleArea { get; set; }
        public static Matrix Transform { get; set; }
        public static CameraMode Mode { get; private set; }
        public static IWalkable Room { get; private set; } // Room to focus on
        public static IList<IVictim> Players { get; set; } // Players to focus

        private static float _animationDuration = 1000;
        private static float _animationTimer = _animationDuration;
        private static float _transitionalZoom;
        private static Vector2 _transitionalPosition;
        private static float _previousZoom;
        private static Vector2 _previousPosition;


        // Zoom levels for...              { Follow, Room, Manual }
        private static readonly float[] minZoom = { 0.05f, 0.25f, 0.05f };
        private static readonly float[] maxZoom = { 1.00f, 1.75f, 16.0f };
        private const float zoomSpeed = 0.1f;
        private const float panSpeed = 1;

        private static float currentMouseWheelValue, previousMouseWheelValue;

        public static void Initialize(Vector2 position, Viewport bounds, CameraMode mode)
        {
            Position = position;
            _transitionalPosition = Position;
            Zoom = 1f;
            _transitionalZoom = Zoom;
            Bounds = bounds.Bounds;
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

        public static void MoveCamera(Vector2 movePosition)
        {
            Vector2 newPosition = Position + movePosition;
            Position = newPosition;
        }

        public static void UpdateZoom(float zoom)
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
            if (Players == null || Players.Count < 1) return;

            // var playersAlive = ((List<IVictim>)Players).FindAll(x => x.WhatAreYou() == X_LevelElements.Victim).ToList();
            var playersAlive = Players;

            if (playersAlive.Count == 0)
            {
                return;
            }

            var left = Players[0].Rect.X;
            var right = Players[0].Rect.X;
            var top = Players[0].Rect.Y;
            var bot = Players[0].Rect.Y;

            Vector2 playerMeanPos = Vector2.Zero;
            foreach (var player in playersAlive)
            {
                playerMeanPos += player.Rect.Location.ToVector2();
                left = Math.Min(player.Rect.X, left);
                right = Math.Max(player.Rect.X, right);
                top = Math.Min(player.Rect.Y, top);
                bot = Math.Max(player.Rect.Y, bot);
            }

            // Update camera position
            playerMeanPos /= playersAlive.Count;
            Position = playerMeanPos;
            // Console.WriteLine(playerMeanPos);

            // Set zoom level to fit all players
            var stretch = Math.Max((float)(right - left) / Bounds.Width, (float)(bot - top) / Bounds.Height);
            UpdateZoom(.55f / stretch);
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
            UpdateZoom(.95f / stretch);
        }

        public static void Update(Viewport bounds, GameTime gameTime)
        {
            if (Input.IsKeyTriggered(Keys.F10))
            {
                Mode = (CameraMode)(((int)Mode + 1) % Enum.GetNames(typeof(CameraMode)).Length);
                Notifications.New("Camera mode switched to " + Mode);
            }

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
            }
            Bounds = bounds.Bounds;

            if (_animationTimer < _animationDuration)
            {
                var animationFraction = _animationTimer / _animationDuration;
                _transitionalPosition = (Position * animationFraction) + _previousPosition * (1f - animationFraction);
                _transitionalZoom = (Zoom * animationFraction) +  _previousZoom * (1f - animationFraction);
                _animationTimer += gameTime.ElapsedGameTime.Milliseconds;
            } else {
                _transitionalPosition = Position;
                _transitionalZoom = Zoom;
            }

            UpdateMatrix();
        }

        private static void startAnimation()
        {
            _previousPosition = _transitionalPosition;
            _previousZoom = _transitionalZoom;
            _animationTimer = 0;
        }

        public static void focusManual()
        {
            Mode = CameraMode.Manual;
        }

        public static void focusOnPlayers()
        {
            startAnimation();
            Mode = CameraMode.Follow;
        }

        public static void focusOnRoom(IWalkable room)
        {
            startAnimation();
            Room = room;
            Mode = CameraMode.Room;
        }
    }
}