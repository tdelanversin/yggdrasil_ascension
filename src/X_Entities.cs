using System;
using System.Collections.Generic;

namespace YGR
{
    public sealed class EnemyEntity
    {
        public const string SimpleEnemy = "SimpleEnemy";
        public const string SlimeEnemy = "SlimeEnemy";
        public const string BossEnemy = "BossEnemy";

        public static Manager_Enemies.EnemyType GetType(EnemyEntity enemy)
        {
            if (enemy.customFields["Type"] == EnemyEntity.SimpleEnemy) return Manager_Enemies.EnemyType.SlimeEnemy;
            if (enemy.customFields["Type"] == EnemyEntity.SlimeEnemy) return Manager_Enemies.EnemyType.SlimeEnemy;
            if (enemy.customFields["Type"] == EnemyEntity.BossEnemy) return Manager_Enemies.EnemyType.BossEnemy;

            return Manager_Enemies.EnemyType.SimpleEnemy;
        }

#pragma warning disable 0649
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
        public Dictionary<string, string> customFields;
#pragma warning restore 0649
    }

    public enum PlayerSpawningPointType
    {
        Chooser,
        Spawner
    }

    public sealed class PlayerEntity
    {
        public const string Nerd = "Nerd";
        public const string Professor = "Professor";
        public const string Mailman = "Mailman";
        public const string Ninja = "Ninja";
        public const string Random = "Random";
        public const string Ghost = "Ghost";

        public const string Chooser = "Chooser";
        public const string Spawner = "Spawner";

        public static PlayerType GetType(PlayerEntity player)
        {
            if (player.customFields["Type"] == PlayerEntity.Nerd) return PlayerType.Nerd;
            if (player.customFields["Type"] == PlayerEntity.Mailman) return PlayerType.Mailman;
            if (player.customFields["Type"] == PlayerEntity.Professor) return PlayerType.Professor;
            if (player.customFields["Type"] == PlayerEntity.Ninja) return PlayerType.Ninja;
            if (player.customFields["Type"] == PlayerEntity.Random) return PlayerType.Random;
            if (player.customFields["Type"] == PlayerEntity.Ghost) return PlayerType.Ghost;

            return PlayerType.Nerd;
        }

        public static PlayerSpawningPointType GetPointType(PlayerEntity player)
        {
            if (player.customFields["PointType"] == Chooser) return PlayerSpawningPointType.Chooser;
            if (player.customFields["PointType"] == Spawner) return PlayerSpawningPointType.Spawner;

            return PlayerSpawningPointType.Spawner;
        }

#pragma warning disable 0649
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
        public Dictionary<string, string> customFields;
#pragma warning restore 0649
    }

    public sealed class PowerUp
    {
        public const string Life = "Life";
        public const string Revive = "Revive";

        // Weapons
        public const string WeaponPistol = "WeaponPistol";
        public const string WeaponShotgun = "WeaponShotgun";
        public const string WeaponKeyboard = "WeaponKeyboard";
        public const string WeaponFunky = "WeaponFunky";

#pragma warning disable 0649
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
        public Dictionary<string, string> customFields;
#pragma warning restore 0649
    }

    public sealed class GenericLdtkEntity
    {
#pragma warning disable 0649
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
#pragma warning restore 0649
    }
}
