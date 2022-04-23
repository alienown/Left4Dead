namespace Left4Dead
{
    public class Pause : PauseSubject
    {
        private bool isPaused;

        public Pause()
        {
            isPaused = false;
        }

        public bool GetState()
        {
            lock (GameData.IsPausedAccessObject)
            {
                return isPaused;
            }
        }

        public void SetState()
        {
            lock (GameData.IsPausedAccessObject)
            {
                if (GetState() == true)
                {
                    isPaused = false;
                    GameData.PauseGameEvent.Set();
                }
                else
                {
                    isPaused = true;
                }
                Notify();
            }
        }
    }
}
