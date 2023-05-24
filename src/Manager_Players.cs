using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace YGR
{
    public static class Manager_Players
    {
        // Player list
        public static List<IPlayer> Players { get; private set; }

        public static void Initialize()
        {
            Players = new List<IPlayer>();
        }

        public static void ClearPlayers()
        {
            Players.Clear();
        }

        public static void ImmobilizePlayers(bool yes)
        {
            foreach(var player in Manager_Players.Players)
            {
                player.Immobilized = yes;
            }
        }

        public static void SetPlayerType(PlayerIndex idx, PlayerType type)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                IPlayer p = Players[i];
                if (p.PlayerIndex == idx)
                {
                    Players[i] = Factory(type, idx, p.Rect.Location.ToVector2(), p.Level, p.ControlLayout);
                }
            }
        }

        public static IPlayer Factory(PlayerType type, PlayerIndex playerIndex, Vector2 position, Y_Level level, ControlLayout controlLayout = ControlLayout.ControllerOnly)
        {
            if (type == PlayerType.Ninja)
                return new Player_Ninja(
                    playerIndex,
                    position,
                    level,
                    null,
                    type,
                    controlLayout
                );
            if (type == PlayerType.Nerd)
                return new Player_NerdyGirl(
                    playerIndex,
                    position,
                    level,
                    null,
                    type,
                    controlLayout
                );
            if (type == PlayerType.Mailman)
                return new Player_Mailman(
                    playerIndex,
                    position,
                    level,
                    null,
                    type,
                    controlLayout
                );
            if (type == PlayerType.Professor)
                return new Player_Professor(
                    playerIndex,
                    position,
                    level,
                    null,
                    type,
                    controlLayout
                );
            else // if (type == PlayerType.Ghost)
            {
                var player = new Player_Ghost(
                    playerIndex,
                    position,
                    level,
                    controlLayout
                );
                return player;
            }
        }

        public static void AddPlayer(
            PlayerType type,
            PlayerIndex playerIndex,
            Vector2 position,
            Y_Level level,
            ControlLayout controlLayout = ControlLayout.ControllerOnly)
        {
            Players.Add(Factory(type, playerIndex, position, level, controlLayout));
        }

        internal static void Update(GameTime gameTime)
        {
            // HACK: .ToList() since players can indirectly modify the Players
            // collection by switching their class in the starting room
            foreach (var player in Players.ToList())
            {
                player.Update(gameTime);
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
            foreach (var player in Players.ToList())
            {
                player.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }
    }
}
