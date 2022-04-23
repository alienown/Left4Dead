using System.Threading;

namespace Left4Dead
{
    public class SniperRifleDrop : Drop
    {
        private int appearTime;
        private Model model;
        private Thread trackingThread;

        public SniperRifleDrop()
        {
            appearTime = GameData.DropAppearTime;
            this.model = new SniperRifleModel(GameData.SniperRifleModel);
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
                if (GameData.Player.Weapons.Exists(w => w.ToString() == "Sniper Rifle"))
                {
                    GameData.Player.CurrentWeapon = GameData.Player.Weapons.Find(w => w.ToString() == "Sniper Rifle");
                    GameData.Player.CurrentWeapon.Ammo = GameData.Player.CurrentWeapon.MaxAmmo;
                }
                else
                {
                    GameData.Player.Weapons.Add(new SniperRifle());
                    GameData.Player.CurrentWeapon = GameData.Player.Weapons.Find(w => w.ToString() == "Sniper Rifle");
                }

                GameData.PlayerAmmoStatusEvent.Set();
                GameData.PlayerCurrentWeaponStatusEvent.Set();
            }
        }
    }
}
