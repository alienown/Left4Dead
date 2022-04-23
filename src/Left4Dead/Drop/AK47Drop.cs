using System.Threading;

namespace Left4Dead
{
    public class AK47Drop : Drop
    {
        private int appearTime;
        private Model model;
        private Thread trackingThread;

        public AK47Drop()
        {
            appearTime = GameData.DropAppearTime;
            this.model = new AK47Model(GameData.Ak47Model);
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
                if (GameData.Player.Weapons.Exists(w => w.ToString() == "AK47"))
                {
                    GameData.Player.CurrentWeapon = GameData.Player.Weapons.Find(w => w.ToString() == "AK47");
                    GameData.Player.CurrentWeapon.Ammo = GameData.Player.CurrentWeapon.MaxAmmo;
                }
                else
                {
                    GameData.Player.Weapons.Add(new AK47());
                    GameData.Player.CurrentWeapon = GameData.Player.Weapons.Find(w => w.ToString() == "AK47");
                }

                GameData.PlayerAmmoStatusEvent.Set();
                GameData.PlayerCurrentWeaponStatusEvent.Set();
            }
        }
    }
}
