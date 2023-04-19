using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace YGR
{
    public static class Manager_Players
    {

        // Player list
        public static IList<IVictim> Players { get; private set; }

        // Sprites
        public static Texture2D SpriteNinja { get; private set; }
        public static Texture2D SpriteBasic { get; private set; }
        public static Texture2D SpriteGhost { get; private set; }
        public static IList<Texture2D> SpriteAimIndicator { get; private set; }

        public static void LoadContent(ContentManager contentManager)
        {
            SpriteNinja = contentManager.Load<Texture2D>("charaset");
            SpriteBasic = contentManager.Load<Texture2D>("tester_60");
            SpriteGhost = contentManager.Load<Texture2D>("ghost");
            SpriteAimIndicator = new List<Texture2D> {
                contentManager.Load<Texture2D>("target_indicator_red"),
                contentManager.Load<Texture2D>("target_indicator_blue"),
                contentManager.Load<Texture2D>("target_indicator_green"),
                contentManager.Load<Texture2D>("target_indicator_yellow"),
            };
        }

        public static void Initialize()
        {
            Players = new List<IVictim>();
        }

        public static void ClearPlayers()
        {
            Players.Clear();
        }

        public static void AddPlayer_SimplePlayer(
            PlayerIndex playerIndex,
            Vector2 position,
            Y_Level level,
            ControlLayout controlLayout = ControlLayout.ControllerOnly)
        {
            Players.Add(new SimplePlayer(
                playerIndex,
                position,
                level,
                new Y_WideGun(),
                controlLayout,
                scale: 1.0f
            ));
        }

        public static void AddPlayer_Ninja(
            PlayerIndex playerIndex,
            Vector2 position,
            Y_Level level,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
        )
        {
            // TODO: clean this up after improving player -> ninja subclassing
            Players.Add(new Ninja(
                    new X_CollisionModel_Victim(1.0f /* mass */, 0.0f /* elastic impact */),
                    playerIndex,
                    SpriteNinja,
                    SpriteGhost,
                    SpriteAimIndicator[(int)playerIndex],
                    0.02f,
                    position,
                    level,
                    new Y_StarterGun(),
                    (int)controlLayout,
                    1.0f
                ));
        }

        internal static void Update(GameTime gameTime)
        {
            foreach (var player in Players)
            {
                player.Update(gameTime);
            }
        }

        internal static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var player in Players)
            {
                player.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        internal static void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var player in Players)
            {
                player.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }
    }
}
