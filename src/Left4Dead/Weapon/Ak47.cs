namespace Left4Dead
{
    public class AK47 : Weapon
    {
        public AK47()
        {
            Ammo = 15;
            MaxAmmo = 15;
            ReloadSpeed = 500;
            BulletSpeed = 10;
            Damage = 10;
            Range = 40;
            BulletModel = new AK47BulletModel(GameData.AK47BulletModel);
        }

        public override string ToString()
        {
            return "AK47";
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
