using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Framework.Media;
using MonoGame.Extended.VideoPlayback;

namespace YGR
{
    public static class Manager_Video
    {
        private static VideoPlayer VideoPlayer;
        private static Video Video;
        public static bool VideoPlaybackNotSupported;

        public static void Initialize(GraphicsDevice graphicsDevice, string videoPath)
        {
            try
            {
                VideoPlayer = new VideoPlayer(graphicsDevice);
                Video = VideoHelper.LoadFromFile(videoPath);
            }
            catch (System.DllNotFoundException)
            {
                // HACK: we want the game to run without the video in this case
                VideoPlaybackNotSupported = true;
                VideoPlayer.Dispose();
            }
        }

        public static void Play()
        {
            if (VideoPlaybackNotSupported)
                return;
            VideoPlayer.Play(Video);
        }

        public static void Stop()
        {
            if (VideoPlaybackNotSupported)
                return;
            VideoPlayer.Stop();
        }

        public static void Dispose()
        {
            if (VideoPlaybackNotSupported)
                return;
            Video.Dispose();
            VideoPlayer.Dispose();
        }

        public static bool Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (VideoPlaybackNotSupported)
                return true;
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
