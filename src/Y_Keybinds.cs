using Microsoft.Xna.Framework.Input;

namespace YGR
{
    public static class Keybinds
    {
        public static Keys P1Up = Keys.W;
        public static Keys P1Left = Keys.A;
        public static Keys P1Down = Keys.S;
        public static Keys P1Right = Keys.D;
        public static Keys ActionOne = Keys.Space;

        public static Keys P2Up = Keys.Up;
        public static Keys P2Left = Keys.Left;
        public static Keys P2Down = Keys.Down;
        public static Keys P2Right = Keys.Right;

        public static Buttons GamePadShoot = Buttons.RightTrigger;
        public static Buttons GamePadAction = Buttons.LeftTrigger;

        public static Keys Enter = Keys.Enter;

        public static Keys CameraMoveUp = Keys.T;
        public static Keys CameraMoveLeft = Keys.F;
        public static Keys CameraMoveDown = Keys.G;
        public static Keys CameraMoveRight = Keys.H;

        /* Debug Area */
        public static Keys CycleCameraMode = Keys.F10;
        public static Keys ToggleFullscreen = Keys.F11;
        public static Keys KillAllEnemies = Keys.Delete;
        public static Keys GodMode = Keys.Back;

        public static Keys OpenAllDoors = Keys.F1;
        public static Keys CloseAllDoors = Keys.F2;
        public static Keys LockAllDoors = Keys.F3;
        public static Keys UnlockAllDoors = Keys.F4;

        public static Keys OpenAllAdjacentDoors = Keys.F5;
        public static Keys CloseAllAdjacentDoors = Keys.F6;
        public static Keys LockAllAdjacentDoors = Keys.F7;
        public static Keys UnlockAllAdjacentDoors = Keys.F8;
    }
}
