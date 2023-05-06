using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YGR
{
    public static class Manager_Players
    {
        public enum PlayerType
        {
            Nerd = 0,
            Ninja,
            Random,
            Ghost
        };

        // Player list
        public static List<IVictim> Players { get; private set; }

        public static void Initialize()
        {
            Players = new List<IVictim>();
        }

        public static void ClearPlayers()
        {
            Players.Clear();
        }

        public static void AddPlayer_NerdyGirl(
            PlayerIndex playerIndex,
            Vector2 position,
            Y_Level level,
            ControlLayout controlLayout = ControlLayout.ControllerOnly)
        {
            Players.Add(new SimplePlayer(
                playerIndex,
                position,
                Manager_Sprites.NewAnimatedSprite_NerdyGirl(),
                level,
                Util.getRandomGun(),
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
            Players.Add(new SimplePlayer(
                playerIndex,
                position,
                Manager_Sprites.NewAnimatedSprite_Ninja(),
                level,
                Util.getRandomGun(),
                controlLayout,
                scale: 1.0f
                ));
        }

        public static void AddPlayer_Random(
            PlayerIndex playerIndex,
            Vector2 position,
            Y_Level level,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
        )
        {
            int r = Util.random.Next() % 2;
            switch (r)
            {
                case 0:
                    AddPlayer_Ninja(playerIndex, position, level, controlLayout);
                    break;
                case 1:
                    AddPlayer_NerdyGirl(playerIndex, position, level, controlLayout);
                    break;
                default:
                    break;
            }
        }

        internal static void Update(GameTime gameTime)
        {
            foreach (var player in Players)
            {
                player.Update(gameTime);

                if (player.LifePoints <= 0 &&
                   !Manager_Sound.playing_sound_effects.ContainsKey(player))

                {
                    SoundEffectInstance death_player_sound = Manager_Sound.Sound_PlayerDeath.CreateInstance();
                    Manager_Sound.playing_sound_effects.Add(player, death_player_sound);
                    death_player_sound.Play();
                }
            }
        }

        internal static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            var playerSorted = Players.OrderBy(t => t.Rect.Y + t.Rect.Height);
            foreach (var player in playerSorted)
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
