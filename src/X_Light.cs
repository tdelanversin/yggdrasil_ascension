using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static YGR.Manager_Light2;

namespace YGR
{
    public class X_IlluminationResources
    {
        public bool InFlight;
        public X_Vector3[] Coords;
        public X_Point3[] Vertices;
        public int NumVerts;
        public int NumLights;
        public int NumCoords;
        public int NumOffset;
        public X_Point3[] Lights;
        public int[] Lighted;
        public bool[] BLighted;
        public int[] Index;
    }
    public class X_Light
    {
        public Color Color { get; }
        public float ShadowP { get; }
        public float Scale { get; }

        private Rectangle _illuminationRect;
        private Rectangle _scaledIlluminationRect;
        private Manager_Light2.X_Point3 _position;
        private Manager_Light2.X_Point3 _scaledPosition;

        public X_Light(Vector3 position, Rectangle illuminationRect, float scale)
        {
            _position = position / scale; // new Vector3(position.Y, position.X, position.Z);
            _scaledPosition = position;
            Scale = scale;
            _illuminationRect = new Rectangle(
                (int)(illuminationRect.X / scale),
                (int)(illuminationRect.Y / scale),
                (int)(illuminationRect.Width / scale),
                (int)(illuminationRect.Height / scale));

            _scaledIlluminationRect = illuminationRect;
        }

        public Manager_Light2.X_Point3 GetScaledPosition()
        {
            return _scaledPosition;
        }

        public Manager_Light2.X_Point3 GetUnscaledPosition()
        {
            return _position;
        }

        public ref Rectangle GetScaledIlluminationRect()
        {
            return ref _scaledIlluminationRect;
        }

        public ref Rectangle GetUnscaledIlluminationRect()
        {
            return ref _illuminationRect;
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            int xpos = (int)(_scaledPosition.X);
            int ypos = (int)(_scaledPosition.Y - _scaledPosition.Z);
            int zpos = (int)(_scaledPosition.Z);
            Factory_Debug.DrawPoint(xpos, ypos, 10, Color.Yellow, spriteBatch);
            Factory_Debug.DrawLine(xpos, ypos, zpos, (float)Math.PI / 2, 5, Color.Yellow, spriteBatch);
            Factory_Debug.DrawRectangle(
                (int)(_scaledIlluminationRect.X),
                (int)(_scaledIlluminationRect.Y),
                (int)(_scaledIlluminationRect.Width),
                (int)(_scaledIlluminationRect.Height), 5, Color.Orange, spriteBatch);
        }

        public string GetIdentifier(IWalkable room)
        {
            Rectangle rect = room.Rect;
            float px = _scaledPosition.X - rect.X;
            float py = _scaledPosition.Y - rect.Y;
            int ipx = _scaledIlluminationRect.X - rect.X;
            int ipy = _scaledIlluminationRect.Y - rect.Y;
            int ipw = _scaledIlluminationRect.Width;
            int iph = _scaledIlluminationRect.Height;
            float scale = Y_Level.GlobalScale;
            return "PX" + px + "PY" + py + "IRX" + ipx + "IRY" + ipy + "IRW" + ipw + "IRH" + iph + "S" + scale;
        }

        public void MoveBy(Point dp)
        {
            var dp3 = new Vector3(dp.X, dp.Y, 0);
            var dp3s = dp3 / Scale;
            var dp2s = new Point((int)(dp.X / Scale), (int)(dp.Y / Scale));

            _position = new Vector3(_position.X, _position.Y, _position.Z) + dp3s;
            _scaledPosition = new Vector3(_scaledPosition.X, _scaledPosition.Y, _scaledPosition.Z) + dp3;

            _illuminationRect.Offset(dp2s);
            _scaledIlluminationRect.Offset(dp);
        }
    }
}
