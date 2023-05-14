using System;
using System.Collections.Generic;
using System.Linq;
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
        public Rectangle Rect { get; set; }
        public int ElementLevel { get; set; }

        // private fields
        public Y_CMRoom Room;
        public Color Color = Color.DarkOrchid;
        public string Label = "";
        public bool InteractionComplete = false;

        public Interactable_Basic(Rectangle bounds, Y_CMRoom room)
        {
            Room = room;

            // Translate into global coordinates via the Room
            Rect = new Rectangle(
                (int)(bounds.X * Y_Level.GlobalScale),
                (int)(bounds.Y * Y_Level.GlobalScale),
                (int)(bounds.Width * Y_Level.GlobalScale),
                (int)(bounds.Height * Y_Level.GlobalScale)
            );
        }

        public virtual List<IPlayer> GetPlayersInside()
        {
            return Manager_Players.Players.FindAll(p => Rectangle.Intersect(Rect, p.Rect) != Rectangle.Empty);
        }

        public void MoveBy(Point offset)
        {
            Rect = new Rectangle(
                Rect.Location.X + offset.X,
                Rect.Location.Y + offset.Y,
                Rect.Width,
                Rect.Height
            );
        }

        public virtual X_LevelElements WhatAreYou() { return X_LevelElements.Interactable; }

        public virtual void Update(GameTime gameTime) { }

        // Just draw the outline and the Label inside it
        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            DrawOutline(gameTime, globalOffset, spriteBatch);
            Vector2 str_size = Fonts.Small.MeasureString(Label);
            Vector2 str_pos = new Vector2(Rect.X + (Rect.Width - str_size.X) / 2, Rect.Y + Rect.Height / 2 - str_size.Y / 2);
            spriteBatch.DrawString(Fonts.Small, Label, str_pos, Color);
        }

        public virtual void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color, spriteBatch);
        }

        internal virtual void Reset()
        {
            InteractionComplete = false;
        }
    }

    // Tutorial field, guiding the players through the start process
    public class Interactable_Tutorialfield : Interactable_Basic
    {
        public Interactable_Tutorialfield(
            Rectangle bounds,
            Y_CMRoom room
        ) : base(bounds, room)
        {
            Color = Color.GhostWhite;
        }

        // Just draw the outline and the Label inside it
        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (Y_Level.State != Y_Level.GamePlayState.Start)
                return;

            DrawOutline(gameTime, globalOffset, spriteBatch);

            int y = Rect.Height / 5 - (int)(Fonts.Small.MeasureString("0").Y / 2);
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

                Vector2 str_size = Fonts.Small.MeasureString(str);
                Vector2 str_pos = new Vector2(Rect.X, Rect.Y + y);
                spriteBatch.DrawString(Fonts.Small, str, str_pos, Color.Lerp(c, Color.Wheat, 0.5f));
                y += Rect.Height / 5;
            }
        }
    }


    // RoomOpener allows opening a room's doors when shot
    public class Interactable_RoomOpener : Interactable_Basic
    {
        enum ButtonState
        {
            Out,
            Half,
            In,
        }

        ButtonState state = ButtonState.Out;

        public Interactable_RoomOpener(
            Rectangle bounds,
            Y_CMRoom room
            ) : base(bounds, room)
        {
            Color = Color.Wheat;
            Label = "Move here to start";
        }

        public void TriggerInteraction(GameTime gameTime)
        {
            // Remove all non-participating players
            Manager_Players.Players.RemoveAll(p => !p.IsActive || (p.ControlLayout == ControlLayout.ControllerOnly && !GamePad.GetState(p.PlayerIndex).IsConnected));

            // Disable all character chooser pickups in the starter room
            foreach (var pu in Room.GetPickUps())
            {
                // HACK: too lazy to implement it differently
                if (pu.Type.ToString().StartsWith("Chooser")) pu.Active = false;
            }

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

            /* One active player can activate as long as others chose their character */
            // foreach (IPlayer p in Manager_Players.Players)
            // {
            //     // Don't care about disconnected players
            //     if (p.ControlLayout == ControlLayout.ControllerOnly && !GamePad.GetState(p.PlayerIndex).IsConnected)
            //         continue;

            //     // One player needs to trigger the field
            //     if (Rect.Contains(p.Rect.Center))
            //         tryTrigger = true;

            //     // Participating players need to choose a character
            //     if (p.IsActive && p is Player_Ghost)
            //         ready = false;
            // }


            /* All active players need to be inside and have picked a character */
            var activePlayers = Manager_Players.Players.FindAll(p => p.IsActive);

            if (activePlayers.Count < 1)
            {
                return;
            }

            foreach (var p in activePlayers)
            {
                if (Rect.Contains(p.Rect.Center + new Point(0, p.Rect.Height / 2)))
                    tryTrigger = true;
                else
                    ready = false;

                if (p is Player_Ghost)
                    ready = false;
            }


            ButtonState statePrev = state;
            if (tryTrigger)
            {
                if (!ready)
                {
                    state = ButtonState.Half;
                    Color = Color.OrangeRed;
                }
                else
                {
                    state = ButtonState.In;
                    TriggerInteraction(gameTime);
                }
            }
            else
            {
                state = ButtonState.Out;
                Color = Color.Wheat;
            }
            if (state != statePrev)
            {
                Manager_Sound.Sound_PlatformActivate.Play();
            }
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Texture2D buttonSprite = Manager_Sprites.ButtonOut;
            if (state == ButtonState.In)
            {
                buttonSprite = Manager_Sprites.ButtonIn;
            }
            else if (state == ButtonState.Half)
            {
                buttonSprite = Manager_Sprites.ButtonHalf;
            }

            spriteBatch.Draw(
                texture: buttonSprite,
                position: Rect.Location.ToVector2(),
                sourceRectangle: null,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: 1,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }


        internal override void Reset()
        {
            InteractionComplete = false;
            state = ButtonState.Out;
        }
    }

    // RoomOpener allows opening a room's doors when shot
    public class Interactable_BossRoomEscaper : Interactable_Basic
    {
        enum ButtonState
        {
            Out,
            Half,
            In
        }

        ButtonState state = ButtonState.Out;

        private int _countDown = 1000;
        private int _secsToEscape = 3;
        private int _countDownCounter;

        public Interactable_BossRoomEscaper(
            Rectangle bounds,
            Y_CMRoom room
            ) : base(bounds, room)
        {
            Color = Color.Green;
            Label = "Get out";
            _countDownCounter = _secsToEscape;
        }

        public void TriggerInteraction(GameTime gameTime)
        {
            // Get all players inside the boss room
            var alivePlayers = Manager_Players.Players.Where(x => x.IsAlive()).ToArray();

            // find the nearest teleportation point in a room that has been discovered and cleared already
            var teleportationTargets = Y_Level.Rooms.Values
                                                    .Where(x => x.WhatAreYou() == X_LevelElements.Room
                                                        && x.IsVisible()
                                                        && ((Y_CMRoom)x).Cleared
                                                        && x.Category != "Gold")
                                                    .Select(x => ((Y_CMRoom)x).TeleporterTarget)
                                                    .ToArray();
            Vector2 midpoint = Room.Rect.Center.ToVector2();

            float mindist = float.MaxValue;
            int index = 0;
            int minIndex = 0;
            foreach (var tt in teleportationTargets)
            {
                var dist = (tt.ToVector2() - midpoint).Length();
                if (dist < mindist)
                {
                    mindist = dist;
                    minIndex = index;
                }
                index++;
            }

            // teleport all players there
            var targetPoint = teleportationTargets[minIndex];
            foreach (var p in alivePlayers)
            {
                p.TeleportTo(targetPoint);
            }

            // unlock all doors back
            Room.OpenAllUnlockedRoomDoors();

            // move camera slowly to new player location
            Camera.SetFocusPlayers(animationDuration: 4000);

            // change the state to FreeRoam (do this before the teleport, otherwise the game ends)
            Y_Level.State = Y_Level.GamePlayState.Escaped;

            // reset the boss room
            Room.InGameReset();

            Label = "Coward!";
            Color = Color.SpringGreen;
            Manager_Sound.Sound_PlatformActivate.Play(1, 0, 0);
            InteractionComplete = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (InteractionComplete) { return; }

            // If the game is already over, no need to use this button
            if (Y_Level.State == Y_Level.GamePlayState.End)
            {
                state = ButtonState.In;
                InteractionComplete = true;
                return;
            }

            bool tryTrigger = false;
            bool ready = true;

            /* All active players need to be inside and have picked a character */
            var activePlayers = Manager_Players.Players.FindAll(p => p.IsActive && (p.WhatAreYou() != X_LevelElements.Ghost));

            if (activePlayers.Count < 1)
            {
                return;
            }

            foreach (var p in activePlayers)
            {
                if (Rect.Contains(p.Rect.Center + new Point(0, p.Rect.Height / 2)))
                {
                    if (_countDownCounter == 0)
                    {
                        tryTrigger = true;
                    }
                    else
                    {
                        Notifications.Clear();
                        Notifications.New("\n\n\n\n", Color.Wheat, 2000);
                        Notifications.New("Escape in: " + _countDownCounter.ToString(), Color.Wheat, 2000, Fonts.Large);
                        _countDown -= gameTime.ElapsedGameTime.Milliseconds;
                        if (_countDown <= 0)
                        {
                            _countDown = 1000;
                            _countDownCounter--;
                        }
                    }
                }
                else
                    ready = false;

                if (p is Player_Ghost)
                    ready = false;
            }


            ButtonState statePrev = state;
            if (tryTrigger)
            {
                if (!ready)
                {
                    state = ButtonState.Half;
                    Color = Color.OrangeRed;
                }
                else
                {
                    state = ButtonState.In;
                    TriggerInteraction(gameTime);
                }
            }
            else
            {
                state = ButtonState.Out;
                Color = Color.Wheat;
                if (!ready)
                {
                    _countDown = 1000;
                    _countDownCounter = _secsToEscape;
                }
            }
            if (state != statePrev)
            {
                Manager_Sound.Sound_PlatformActivate.Play();
            }
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Texture2D buttonSprite = Manager_Sprites.EscapeButtonOut;
            if (state == ButtonState.In)
            {
                buttonSprite = Manager_Sprites.EscapeButtonIn;
            }
            else if (state == ButtonState.Half)
            {
                buttonSprite = Manager_Sprites.EscapeButtonHalf;
            }

            spriteBatch.Draw(
                texture: buttonSprite,
                position: Rect.Location.ToVector2(),
                sourceRectangle: null,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: 1,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }


        internal override void Reset()
        {
            InteractionComplete = false;
            state = ButtonState.Out;
        }
    }
}
