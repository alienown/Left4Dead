using System.Threading;

namespace Left4Dead
{
    public class HealthPack : Drop
    {
        private int appearTime;
        private Model model;
        private Thread trackingThread;

        public HealthPack()
        {
            appearTime = GameData.DropAppearTime;
            model = new HealthPackModel(GameData.HealthPackModel);
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
            lock (GameData.PlayerHealthAccess)
            {
                if (GameData.Player.Health + 20 <= 100)
                {
                    GameData.Player.Health += 20;
                }
                else
                {
                    GameData.Player.Health = 100;
                }
                GameData.PlayerHealthStatusEvent.Set();
            }
        }
    }
}
