using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Slime : Enemy_Basic
    {
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

            _hitColor = Color.OrangeRed;
            _regularColor = new Color(144, 252, 127); // Slimey green, picked from the colored png files
            _color = _regularColor;

            var _mass = 2.0f;
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            // Collision bounds
            int height = 55;
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
