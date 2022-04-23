using System.Threading;

namespace Left4Dead
{
    public abstract class Drop
    {
        public abstract void AddValue();

        public abstract Thread GetTrackingThread();

        public abstract void SetTrackingThread(Thread trackingThread);

        public abstract int GetAppearTime();

        public abstract Model GetModel();
    }
}
