using System.Collections.Generic;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        public virtual List<IPlayer> GetPlayersInside()
        {
            return Manager_Players.Players.FindAll(p => Rectangle.Intersect(Rect, p.Rect) != Rectangle.Empty);
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

    // Tutorial field, guiding the players through the start process
    public class Interactable_Tutorialfield : Interactable_Basic
    {
        public Interactable_Tutorialfield(
            Rectangle bounds,
            Y_Level level,
            Y_CMRoom room
        ) : base(bounds, level, room)
        {
            Color = Color.GhostWhite;
        }

        // Just draw the outline and the Label inside it
        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (Level.State != Y_Level.GamePlayState.Start)
                return;

            DrawOutline(gameTime, globalOffset, spriteBatch);

            int y = Rect.Height / 5 - (int)(Fonts.Normal.MeasureString("0").Y / 2);
            List<string> text = new List<string> { };
            foreach (IPlayer p in Manager_Players.Players)
            {
                string str = " P" + (int)p.PlayerIndex + ": ";
                Color c;
                if (p.ControlLayout == ControlLayout.ControllerOnly && !GamePad.GetState(p.PlayerIndex).IsConnected)
                {
                    str += "n/a";
                    c = Color.White;
                }
                else if (!p.IsActive)
                {
                    str += "Inactive (move to register)";
                    c = Color.Orange;
                }
                else if (p is Player_Ghost)
                {
                    str += "Active (select character)";
                    c = Color.HotPink;
                }
                else
                {
                    str += "Ready";
                    c = Color.LimeGreen;
                }

                Vector2 str_size = Fonts.Normal.MeasureString(str);
                Vector2 str_pos = new Vector2(Rect.X, Rect.Y + y);
                spriteBatch.DrawString(Fonts.Normal, str, str_pos, Color.Lerp(c, Color.Wheat, 0.5f));
                y += Rect.Height / 5;
            }
        }
    }


    // RoomOpener allows opening a room's doors when shot
    public class Interactable_RoomOpener : Interactable_Basic
    {

        public Interactable_RoomOpener(
            Rectangle bounds,
            Y_Level level,
            Y_CMRoom room
            ) : base(bounds, level, room)
        {
            Color = Color.Wheat;
            Label = "Move here to start";
        }

        public void TriggerInteraction(GameTime gameTime)
        {
            // Remove all non-participating players
            Manager_Players.Players.RemoveAll(p => !p.IsActive || (p.ControlLayout == ControlLayout.ControllerOnly && !GamePad.GetState(p.PlayerIndex).IsConnected));

            // Disable all pickups in the starter room
            Room.PickUps.ForEach(pu => pu.Active = false);

            Label = "Have fun! :)";
            Color = Color.SpringGreen;
            Manager_Sound.Sound_PlatformActivate.Play(1, 0, 0);
            InteractionComplete = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (InteractionComplete) { return; }

            bool tryTrigger = false;
            bool ready = true;

            foreach (IPlayer p in Manager_Players.Players)
            {
                // Don't care about disconnected players
                if (p.ControlLayout == ControlLayout.ControllerOnly && !GamePad.GetState(p.PlayerIndex).IsConnected)
                    continue;

                // One player needs to trigger the field
                if (Rect.Contains(p.Rect.Center))
                    tryTrigger = true;

                // Participating players need to choose a character
                if (p.IsActive && p is Player_Ghost)
                    ready = false;
            }

            if (tryTrigger)
            {
                if (!ready)
                {
                    Color = Color.OrangeRed;
                }
                else
                {
                    TriggerInteraction(gameTime);
                }
            }
            else
            {
                Color = Color.Wheat;
            }
        }
    }
}
