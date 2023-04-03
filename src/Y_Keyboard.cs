using Microsoft.Xna.Framework.Input;

/*
 * Static methods to use single press keybindings
 * https://community.monogame.net/t/one-shot-key-press/11669
 */

namespace YGR
{
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