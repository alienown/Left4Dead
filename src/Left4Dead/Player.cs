using System;
using System.Collections.Generic;
using System.Linq;

namespace Left4Dead
{
    public class Player : IDisposable
    {
        private static Player player = null;

        public Model PlayerModel { get; set; }
        public Direction Direction { get; set; }
        public Position Position { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public List<Weapon> Weapons { get; set; }
        public Weapon CurrentWeapon { get; set; }
        public bool IsStunned { get; set; }
        public bool IsReloaded { get; set; }
        public int Score { get; set; }
        public string Name { get; set; }

        private Player()
        {
            MaxHealth = 100;
            Score = 0;
            IsStunned = false;
            IsReloaded = true;

            Name = GameData.PlayerName;
            PlayerModel = new PlayerModel(GameData.PlayerModel);
            Score = 0;
            Position = new Position(65, 25);
            Health = 100;
            Direction = Direction.DOWN;
            Weapons = new List<Weapon>() { new Pistol() };
            CurrentWeapon = Weapons.ElementAt(0);
        }

        public static Player GetPlayer()
        {
            if (player == null)
            {
                lock (GameData.PlayerObjectAccess)
                {
                    if (player == null)
                    {
                        player = new Player();
                    }
                }
            }

            return player;
        }

        public void Dispose()
        {
            player = null;
        }
    }
}
