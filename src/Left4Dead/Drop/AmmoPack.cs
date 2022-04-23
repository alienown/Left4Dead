using System.Threading;

namespace Left4Dead
{
    public class AmmoPack : Drop
    {
        private int appearTime;
        private Model model;
        private Thread trackingThread;

        public AmmoPack()
        {
            appearTime = GameData.DropAppearTime;
            model = new AmmoPackModel(GameData.AmmoPackModel);
        }

        public override void SetTrackingThread(Thread trackingThread)
        {
            this.trackingThread = trackingThread;
        }

        public override int GetAppearTime()
        {
            return appearTime;
        }

        public override Model GetModel()
        {
            return model;
        }

        public override Thread GetTrackingThread()
        {
            return trackingThread;
        }

        public override void AddValue()
        {
            lock (GameData.WeaponAccessObject)
            {
                if (GameData.Player.CurrentWeapon.ToString() == "Pistol")
                {
                    if (GameData.Player.Weapons.Exists(w => w.ToString() == "AK47"))
                    {
                        Weapon ak47 = GameData.Player.Weapons.Find(w => w.ToString() == "AK47");
                        if (ak47.Ammo + 5 <= ak47.MaxAmmo)
                        {
                            ak47.Ammo += 5;
                        }
                        else
                        {
                            ak47.Ammo = ak47.MaxAmmo;
                        }
                    }
                    else if (GameData.Player.Weapons.Exists(w => w.ToString() == "Sniper Rifle"))
                    {
                        Weapon sniperRifle = GameData.Player.Weapons.Find(w => w.ToString() == "Sniper Rifle");
                        if (sniperRifle.Ammo + 3 <= sniperRifle.MaxAmmo)
                        {
                            sniperRifle.Ammo += 3;
                        }
                        else
                        {
                            sniperRifle.Ammo = sniperRifle.MaxAmmo;
                        }
                    }
                }
                else if (GameData.Player.CurrentWeapon.ToString() == "AK47")
                {
                    if (GameData.Player.CurrentWeapon.Ammo + 5 <= GameData.Player.CurrentWeapon.MaxAmmo)
                    {
                        GameData.Player.CurrentWeapon.Ammo += 5;
                    }
                    else
                    {
                        GameData.Player.CurrentWeapon.Ammo = GameData.Player.CurrentWeapon.MaxAmmo;
                    }
                }
                else if (GameData.Player.CurrentWeapon.ToString() == "Sniper Rifle")
                {
                    if (GameData.Player.CurrentWeapon.Ammo + 3 <= GameData.Player.CurrentWeapon.MaxAmmo)
                    {
                        GameData.Player.CurrentWeapon.Ammo += 3;
                    }
                    else
                    {
                        GameData.Player.CurrentWeapon.Ammo = GameData.Player.CurrentWeapon.MaxAmmo;
                    }
                }
                GameData.PlayerAmmoStatusEvent.Set();
            }
        }
    }
}
