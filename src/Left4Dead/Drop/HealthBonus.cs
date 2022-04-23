using System;
using System.Threading;

namespace Left4Dead
{
    public class HealthBonus : Bonus
    {
        public HealthBonus(Drop drop) : base(drop)
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
            model.BackgroundColor = ConsoleColor.Green;
            return model;
        }

        public override void AddValue()
        {
            lock (GameData.PlayerHealthAccess)
            {
                if (GameData.Player.Health + 50 <= 100)
                {
                    GameData.Player.Health += 50;
                }
                else
                {
                    GameData.Player.Health = 100;
                }
                base.AddValue();
                GameData.PlayerHealthStatusEvent.Set();
            }
        }
    }
}
