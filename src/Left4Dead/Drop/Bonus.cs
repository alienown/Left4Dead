using System.Threading;

namespace Left4Dead
{
    public class Bonus : Drop
    {
        protected Drop Drop;

        public Bonus(Drop drop)
        {
            this.Drop = drop;
        }

        public override void AddValue()
        {
            Drop.AddValue();
        }

        public override int GetAppearTime()
        {
            return Drop.GetAppearTime();
        }

        public override Model GetModel()
        {
            return Drop.GetModel();
        }

        public override Thread GetTrackingThread()
        {
            return Drop.GetTrackingThread();
        }

        public override void SetTrackingThread(Thread trackingThread)
        {
            Drop.SetTrackingThread(trackingThread);
        }
    }
}
