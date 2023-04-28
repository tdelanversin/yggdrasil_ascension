using System.Collections.Generic;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    // Basic interactable.
    // Does nothing really, but provides some useful basic fields and constructor.
    public class Interactable_Basic : IGameElement
    {
        // IGameElement fields
        public float Scale { get; }
        public Rectangle Rect { get; set; }

        // private fields
        public Y_Level Level;
        public Y_CMRoom Room;
        public Color Color = Color.DarkOrchid;
        public string Label = "";
        public bool InteractionComplete = false;

        public Interactable_Basic(Rectangle bounds, Y_Level level, Y_CMRoom room)
        {
            Level = level;
            Room = room;

            // Translate into global coordinates via the Room
            Rect = new Rectangle(
                Room.Rect.Location.X + bounds.X * Level.TileWidth,
                Room.Rect.Location.Y + bounds.Y * Level.TileHeight,
                bounds.Width * Level.TileWidth,
                bounds.Height * Level.TileHeight
            );
        }

        public virtual X_LevelElements WhatAreYou() { return X_LevelElements.Interactable; }

        public virtual void Update(GameTime gameTime) { }

        // Just draw the outline and the Label inside it
        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            DrawOutline(gameTime, globalOffset, spriteBatch);
            Vector2 str_size = Fonts.Normal.MeasureString(Label);
            Vector2 str_pos = new Vector2(Rect.X + (Rect.Width - str_size.X) / 2, Rect.Y + Rect.Height / 2 - str_size.Y / 2);
            spriteBatch.DrawString(Fonts.Normal, Label, str_pos, Color);
        }

        public virtual void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color, spriteBatch);
        }
    }


    // Simple field for players to stand in
    public class Interactable_PlayerField : Interactable_Basic
    {
        public List<IVictim> PlayersInside = new List<IVictim> { };
        private string BaseLabel = "Move here!";

        public Interactable_PlayerField(
            Rectangle bounds,
            Y_Level level,
            Y_CMRoom room
        ) : base(bounds, level, room)
        {
            Label = BaseLabel;
        }

        public virtual List<IVictim> GetPlayersInside()
        {
            return ((List<IVictim>)Manager_Players.Players).FindAll(p => Rectangle.Intersect(Rect, p.Rect) != Rectangle.Empty);
        }

        public override void Update(GameTime gameTime)
        {
            var playersInside = GetPlayersInside();

            if (playersInside.FindAll(p => !PlayersInside.Contains(p)).Count > 0)
            {
                // Play sound effect when a new player enters the field
                Manager_Sound.Sound_GunCocking.Play();
            }

            PlayersInside = playersInside;
            Label = BaseLabel + "\nPlayers: " + PlayersInside.Count;

            // Light up if any player stands inside
            if (PlayersInside.Count > 0)
            {
                Color = Color.Azure;
            }
            else
            {
                Color = Color.DarkGoldenrod;
            }
        }
    }


    // RoomOpener allows opening a room's doors when shot
    public class Interactable_RoomOpener : Interactable_Basic
    {
        public Interactable_PlayerField PlayerField;

        public Interactable_RoomOpener(
            Rectangle bounds,
            Y_Level level,
            Y_CMRoom room,
            Interactable_PlayerField playerField
            ) : base(bounds, level, room)
        {
            Color = Color.DarkGoldenrod;
            Label = "Shoot me when all\n players ready! ";
            PlayerField = playerField;
        }

        public void TriggerInteraction(GameTime gameTime)
        {
            List<IVictim> selectedPlayers = PlayerField.PlayersInside;

            // If not a single player manages to stand in the field, we're not starting the game
            if (selectedPlayers.Count < 1) { return; }

            Manager_Players.Players.RemoveAll(p => !selectedPlayers.Contains(p));
            Room.OpenDoorsAndAdjacentRooms();
            Label = "Go get 'em! :)";
            Color = Color.SpringGreen;
            Manager_Sound.Sound_PlatformActivate.Play(1, 0, 0);
            InteractionComplete = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (InteractionComplete) { return; }

            // Amazing "collision detection"
            foreach (var projectile in Manager_Projectile.GetProjectiles())
            {
                // Can only be triggered by a player standing in the field
                if (projectile.WhoFiredMe is not IVictim) { return; }
                if (!PlayerField.PlayersInside.Contains((IVictim)projectile.WhoFiredMe)) { return; }

                if (Rect.Contains(projectile.Rect))
                {
                    TriggerInteraction(gameTime);
                    return;
                }
            }
        }
    }
}
