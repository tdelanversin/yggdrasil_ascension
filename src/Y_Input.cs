using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

/*
 * Static methods to use single press keybindings
 * https://community.monogame.net/t/one-shot-key-press/11669
 */

namespace YGR
{
    // Global input class to rule them all
    public static class Input
    {
        static KeyboardState currentKeyState;
        static KeyboardState previousKeyState;
        static MouseState currentMouseState;
        static MouseState previousMouseState;
        static IList<GamePadState> currentGamePadState;
        static IList<GamePadState> previousGamePadState;
        static Game Game;

        internal static void Initialize(Game game)
        {
            Game = game;
            previousGamePadState = new List<GamePadState>(4);
            currentGamePadState = new List<GamePadState>(4);

            for (int i = 0; i < 4; i++)
            {
                previousGamePadState.Add(GamePad.GetState(i));
                currentGamePadState.Add(GamePad.GetState(i));
            }
        }

        /// <summary>
        /// Update all states. Only call this once per game update.
        /// </summary>
        /// <returns></returns>
        public static void Update()
        {
            if (!Game.IsActive)
            {
                return;
            }

            // The previous states are needed to figure out when something is
            // pressed __initially__, to bind a actions to a key/button without
            // triggering them multiple times in between letting go of it.
            previousKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            for (int i = 0; i < 4; i++)
            {
                previousGamePadState[i] = currentGamePadState[i];
                currentGamePadState[i] = GamePad.GetState(i);
            }
        }

        public static bool AnyGamePadButtonPressed()
        {
            return currentGamePadState
                .Any(x =>
                    x.IsButtonDown(Buttons.A) || x.IsButtonDown(Buttons.B) || x.IsButtonDown(Buttons.Back) ||
                    x.IsButtonDown(Buttons.BigButton) || x.IsButtonDown(Buttons.DPadDown) || x.IsButtonDown(Buttons.DPadLeft) ||
                    x.IsButtonDown(Buttons.DPadRight) || x.IsButtonDown(Buttons.DPadUp) || x.IsButtonDown(Buttons.LeftShoulder) ||
                    x.IsButtonDown(Buttons.LeftTrigger) || x.IsButtonDown(Buttons.RightShoulder) || x.IsButtonDown(Buttons.RightTrigger) ||
                    x.IsButtonDown(Buttons.Start) || x.IsButtonDown(Buttons.X) || x.IsButtonDown(Buttons.Y));
        }

        public static bool AnythingPressed()
        {
            return (currentKeyState.GetPressedKeyCount() > 0 || AnyGamePadButtonPressed()) && !IsKeyDown(Keybinds.ToggleFullscreen);
        }

        public static bool IsKeyDown(Keys key)
        {
            return currentKeyState.IsKeyDown(key);
        }

        public static bool IsButtonDown(PlayerIndex gamePadIndex, Buttons button)
        {
            return currentGamePadState[(int)gamePadIndex].IsButtonDown(button);
        }

        public static bool IsKeyTriggered(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

        public static bool IsButtonTriggered(PlayerIndex gamePadIndex, Buttons button)
        {
            return currentGamePadState[(int)gamePadIndex].IsButtonDown(button) && !previousGamePadState[(int)gamePadIndex].IsButtonDown(button);
        }

        public static bool IsButtonDownAny(Buttons button)
        {
            foreach (var idx in new List<PlayerIndex> { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four })
            {
                if (IsButtonDown(idx, button)) { return true; }
            }
            return false;
        }

        public static bool IsButtonTriggeredAny(Buttons button)
        {
            foreach (var idx in new List<PlayerIndex> { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four })
            {
                if (IsButtonTriggered(idx, button)) { return true; }
            }
            return false;
        }

        public static Point GetMousePosition()
        {
            return currentMouseState.Position;
        }

        public static bool HasMouseMoved()
        {
            return currentMouseState.Position != previousMouseState.Position;
        }

        public static bool IsLeftMousePressed()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool IsRightMousePressed()
        {
            return currentMouseState.RightButton == ButtonState.Pressed;
        }

        public static bool IsLeftMouseClick()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton != ButtonState.Pressed;
        }

        public static bool IsRightMouseClick()
        {
            return currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed;
        }

        public static bool IsMiddleMouseClick()
        {
            return currentMouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton != ButtonState.Pressed;
        }

        public static bool HasGamePadStateChanged(PlayerIndex gamePadIndex)
        {
            return currentGamePadState[(int)gamePadIndex] != previousGamePadState[(int)gamePadIndex];
        }

        public static bool HasMouseStateChanged()
        {
            return currentMouseState != previousMouseState;
        }

        public static bool HasKeyboardStateChanged()
        {
            return currentKeyState != previousKeyState;
        }

        internal static Vector2 GetMousePositionInGame()
        {
            var mousePositionWindow = Input.GetMousePosition().ToVector2();

            // Scale by camera zoom level
            var mousePositionGame = mousePositionWindow / Camera.Zoom;

            // Offset by camera visible area bounds
            return mousePositionGame + Camera.VisibleArea.Location.ToVector2();
        }
    }
}
