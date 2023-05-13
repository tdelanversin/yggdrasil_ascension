using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Slime_Spiky : Enemy_Basic
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

        public Enemy_Slime_Spiky(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level
        ) : base(position, sprite, level)
        {
            LifePointsMax = 9;
            LifePoints = LifePointsMax;
            fleeingHPTreshold = LifePointsMax / 2;

            Name = "Slime Spiky";
            Gun = new Gun_BasicEnemy(this);

            Color = SlimeyColors[Util.random.Next(SlimeyColors.Count)]; // Slimey green, picked from the colored png files
            _currentColor = Color;
            _hitColor = Color.DarkRed;

            var _mass = 2.0f;
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            // Collision bounds
            int height = 45;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);

            // Offset the enitity to center it on the spawner tile
            _position = position - new Vector2(width / 2, height / 2);
            _rect = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero;
        }

        public void ChangeColor(Color color)
        {
            Color = color;
            _currentColor = color;
        }
    }
}
