using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Slime : Enemy_Basic
    {
        protected static List<Color> SlimeyColors = new List<Color> {
            new Color(252, 45, 218),
            new Color(142, 176, 0),
            new Color(144, 252, 127),
            new Color(107, 253, 22),
            new Color(164, 72, 179),
            new Color(103, 28, 166),
            new Color(121, 238, 96),
            new Color(96, 238, 168),
        };

        public Enemy_Slime(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level,
            IList<IVictim> players
        ) : base(position, sprite, level, players)
        {
            LifePointsMax = 15;
            LifePoints = LifePointsMax;
            fleeingHPTreshold = 0; // Gigachad never flees

            Name = "Slime";
            Gun = new Gun_BasicEnemy();

            Color = SlimeyColors[Util.random.Next(SlimeyColors.Count)]; // Slimey green, picked from the colored png files
            _currentColor = Color;
            _hitColor = Color.DarkRed;

            var _mass = 2.0f;
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            // Collision bounds
            int height = 45;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);

            _rect = new Rectangle(
                (int)position.X - height / 2,
                (int)position.Y - width / 2,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero;
        }
    }
}
