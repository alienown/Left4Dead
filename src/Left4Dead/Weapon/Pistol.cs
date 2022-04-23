namespace Left4Dead
{
    public class Pistol : Weapon
    {
        public Pistol()
        {
            Ammo = -1;
            MaxAmmo = -1;
            ReloadSpeed = 1000;
            BulletSpeed = 10;
            Damage = 5;
            Range = 15;
            BulletModel = new PistolBulletModel(GameData.PistolBulletModel);
        }

        public override string ToString()
        {
            return "Pistol";
        }

        public override string ShowCurrentAmmo()
        {
            lock (GameData.WeaponAccessObject)
            {
                return "UNLIMITED";
            }
        }
    }
}
