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
    }

}
