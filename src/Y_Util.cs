using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Useful static methods
 */

namespace YGR
{
    public static class Util
    {
        public const int RES_X = 1280;
        public const int RES_Y = 720;

        public static void ToggleFullscreen(GraphicsDeviceManager gdm, GameWindow window)
        {
            // TODO: hidpi scaling
            if (gdm.IsFullScreen)
            {
                gdm.PreferredBackBufferWidth = RES_X;
                gdm.PreferredBackBufferHeight = RES_Y;
                gdm.IsFullScreen = false;
                Logger.Info("Turning fullscreen OFF. Resolution: " + RES_X.ToString() + "x" + RES_Y.ToString());
            }
            else
            {
                gdm.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                gdm.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                gdm.IsFullScreen = true;
                Logger.Info("Turning fullscreen ON. Resolution: " + gdm.PreferredBackBufferWidth.ToString() + "x" + gdm.PreferredBackBufferHeight.ToString());
            }
            gdm.ApplyChanges();
        }
    }
}
