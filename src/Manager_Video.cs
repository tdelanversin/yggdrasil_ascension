using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Framework.Media;
using MonoGame.Extended.VideoPlayback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public static class Manager_Video
    {
        private static VideoPlayer VideoPlayer;
        private static Video Video;

        public static void Initialize(GraphicsDevice graphicsDevice, string videoPath)
        {
            VideoPlayer = new VideoPlayer(graphicsDevice);
            Video = VideoHelper.LoadFromFile(videoPath);
        }

        public static void Play()
        {
            VideoPlayer.Play(Video);
        }

        public static void Stop()
        {
            VideoPlayer.Stop();
        }

        public static void Dispose()
        {
            Video.Dispose();
            VideoPlayer.Dispose();
        }

        public static bool Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            try
            {
                if (VideoPlayer.State == MediaState.Playing)
                {
                    // Frame and audio synchronization is automatic.
                    var texture = VideoPlayer.GetTexture();

                    if (texture != null)
                    {
                        spriteBatch.Draw(texture, Camera.Bounds, Color.White);
                    }
                    return false;
                }
                else
                {
                    VideoPlayer.Stop();
                    return true;
                }
            }
            catch
            {
                VideoPlayer.Stop();
                return true;
            }

            // Do NOT dispose the obtained texture. It is a image cache so it is not recreated every call.
        }
    }
}
