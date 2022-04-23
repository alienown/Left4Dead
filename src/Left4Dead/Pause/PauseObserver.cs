namespace Left4Dead
{
    public abstract class PauseObserver
    {
        protected bool IsPaused { get; set; }

        protected Pause Pause { get; set; }

        public abstract void Update();
    }
}
