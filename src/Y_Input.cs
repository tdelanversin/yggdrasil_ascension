using System.Collections.Generic;
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

        internal static void Initialize()
        {
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
        public static bool IsKeyDown(Keys key)
        {
            return currentKeyState.IsKeyDown(key);
        }

        public static bool IsButtonDown(int gamePadIndex, Buttons button)
        {
            return currentGamePadState[gamePadIndex].IsButtonDown(button);
        }

        public static bool IsKeyTriggered(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

        public static bool IsButtonTriggered(int gamePadIndex, Buttons button)
        {
            return currentGamePadState[gamePadIndex].IsButtonDown(button) && !previousGamePadState[gamePadIndex].IsButtonDown(button);
        }

        public static bool HasMouseMoved()
        {
            return currentMouseState.Position != previousMouseState.Position;
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
    }
}
