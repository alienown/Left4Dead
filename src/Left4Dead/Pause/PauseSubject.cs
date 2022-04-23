using System.Collections.Generic;

namespace Left4Dead
{
    public abstract class PauseSubject
    {
        private List<PauseObserver> observers;

        public PauseSubject()
        {
            observers = new List<PauseObserver>();
        }

        public void Attach(PauseObserver o)
        {
            lock (GameData.IsPausedAccessObject)
            {
                observers.Add(o);
            }
        }

        public void Detach(PauseObserver o)
        {
            lock (GameData.IsPausedAccessObject)
            {
                observers.Remove(o);
            }
        }

        protected void Notify()
        {
            lock (GameData.IsPausedAccessObject)
            {
                foreach (PauseObserver o in observers)
                {
                    o.Update();
                }
            }
        }
    }
}
