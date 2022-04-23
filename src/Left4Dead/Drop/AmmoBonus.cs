using System;
using System.Threading;

namespace Left4Dead
{
    public class AmmoBonus : Bonus
    {
        public AmmoBonus(Drop drop) : base(drop)
        {
        }

        public override Thread GetTrackingThread()
        {
            return base.GetTrackingThread();
        }

        public override void SetTrackingThread(Thread trackingThread)
        {
            base.SetTrackingThread(trackingThread);
        }

        public override int GetAppearTime()
        {
            return base.GetAppearTime();
        }

        public override Model GetModel()
        {
            Model model = base.GetModel();
            model.ModelColor = ConsoleColor.Yellow;
            return model;
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
                        if (ak47.Ammo + 3 <= ak47.MaxAmmo)
                        {
                            ak47.Ammo += 3;
                        }
                        else
                        {
                            ak47.Ammo = ak47.MaxAmmo;
                        }
                    }
                    else if (GameData.Player.Weapons.Exists(w => w.ToString() == "Sniper Rifle"))
                    {
                        Weapon sniperRifle = GameData.Player.Weapons.Find(w => w.ToString() == "Sniper Rifle");
                        if (sniperRifle.Ammo + 1 <= sniperRifle.MaxAmmo)
                        {
                            sniperRifle.Ammo += 1;
                        }
                        else
                        {
                            sniperRifle.Ammo = sniperRifle.MaxAmmo;
                        }
                    }
                }
                else if (GameData.Player.CurrentWeapon.ToString() == "AK47")
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
                else if (GameData.Player.CurrentWeapon.ToString() == "Sniper Rifle")
                {
                    if (GameData.Player.CurrentWeapon.Ammo + 1 <= GameData.Player.CurrentWeapon.MaxAmmo)
                    {
                        GameData.Player.CurrentWeapon.Ammo += 1;
                    }
                    else
                    {
                        GameData.Player.CurrentWeapon.Ammo = GameData.Player.CurrentWeapon.MaxAmmo;
                    }
                }
                GameData.PlayerAmmoStatusEvent.Set();
            }
            base.AddValue();
        }
    }
}
