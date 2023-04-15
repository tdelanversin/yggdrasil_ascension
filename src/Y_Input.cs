using System;
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
                previousGamePadState.Add(Microsoft.Xna.Framework.Input.GamePad.GetState(i));
                currentGamePadState.Add(Microsoft.Xna.Framework.Input.GamePad.GetState(i));
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
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            for (int i = 0; i < 4; i++)
            {
                previousGamePadState[i] = currentGamePadState[i];
                currentGamePadState[i] = Microsoft.Xna.Framework.Input.GamePad.GetState(i);
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
    }

    // TODO: Phase out, it's superceeded by the Input class
    // Leaving here only for backwards compatibility
    public static class Keyboard
    {
        static KeyboardState currentKeyState;
        static KeyboardState previousKeyState;

        /// <summary>
        /// Get new keyboard state. Only call this once per game update.
        /// </summary>
        /// <returns></returns>
        public static KeyboardState Update()
        {
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            return currentKeyState;
        }

        /// <summary>
        /// Returns true if a key is currently being pressed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsPressed(Keys key)
        {
            return currentKeyState.IsKeyDown(key);
        }

        /// <summary>
        /// Returns true exactly once when a key is pressed, holding down will not re-trigger
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasBeenPressed(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }
    }
}