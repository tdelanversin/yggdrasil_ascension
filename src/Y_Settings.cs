using System;

namespace YGR
{

    public static class Settings
    {
        public static bool Lighting;
        public static bool Outlines;

        public static void Initialize()
        {
            // Defaults
            Lighting = true;
            Outlines = false;
        }

        internal static bool ToggleLighting()
        {
            Lighting = !Lighting;
            return Lighting;
        }

        internal static bool ToggleOutlines()
        {
            Outlines = !Outlines;
            return Outlines;
        }
    }

}
