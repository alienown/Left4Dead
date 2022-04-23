namespace Left4Dead
{
    public class SniperRifle : Weapon
    {
        public SniperRifle()
        {
            Ammo = 5;
            MaxAmmo = 5;
            ReloadSpeed = 3000;
            BulletSpeed = 10;
            Damage = 20;
            Range = 130;
            BulletModel = new SniperRifleBulletModel(GameData.SniperRifleBulletModel);
        }

        public override string ToString()
        {
            return "Sniper Rifle";
        }

        public override string ShowCurrentAmmo()
        {
            lock (GameData.WeaponAccessObject)
            {
                return GameData.Player.CurrentWeapon.Ammo.ToString();
            }
        }
    }
}
