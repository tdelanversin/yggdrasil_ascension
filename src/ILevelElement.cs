/*
 * Interface to be implemented by everything that should be able to answer the question
 * 
 * These should be all things where a movable sprite (player, projectile, ... ) can collide with
 */

namespace YGR
{
    public interface ILevelElement : IGameElement
    {
        public X_LevelElements WhatAreYou();
    }
}
