namespace Left4Dead
{
    public abstract class Weapon
    {
        public int ReloadSpeed { get; set; }
        public int BulletSpeed { get; set; }
        public int Ammo { get; set; }
        public int Damage { get; set; }
        public int Range { get; set; }
        public int MaxAmmo { get; set; }
        public Model BulletModel { get; set; }

        public abstract string ShowCurrentAmmo();
    }
}
