using System;
using System.Threading;

namespace Left4Dead
{
    public abstract class Zombie : PauseObserver
    {
        private bool drop;

        public IZombieState ZombieState { get; set; }
        public Direction Direction { get; set; }
        public Thread ZombieThread { get; set; }
        public bool IsPushable { get; set; }
        public int Health { get; set; }
        public Position ZombiePosition { get; set; }
        public int WalkSpeed { get; set; }
        public int AttackSpeed { get; set; }
        public int AttackDamage { get; set; }
        public int MaxHealth { get; set; }
        public Model ZombieModel { get; set; }

        public Zombie(Pause pause)
        {
            drop = true;
            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
        }

        public abstract void MakeUpset();

        public abstract void Kill();

        public void SetDrop(bool set)
        {
            drop = set;
        }

        public bool IsPlayerAround()
        {
            lock (GameData.ConsoleAccessObject)
            {
                if (Math.Abs(ZombiePosition.X - GameData.Player.Position.X) <= 1 && Math.Abs(ZombiePosition.Y - GameData.Player.Position.Y) <= 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Position FindSpawnPosition()
        {
            Position position;
            Collision collision;

            do
            {
                position = new Position(GameData.GenerateRandomNumber(19, 82), GameData.GenerateRandomNumber(6, 44));
                collision = GameData.CheckForCollision(position);
            }
            while (collision != Collision.NONE);

            return position;
        }

        public void LiveTemplateMethod()
        {
            try
            {
                do
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                    FollowPlayer();
                    Thread.Sleep(WalkSpeed);
                } while (Health > 0);
            }
            catch (ThreadInterruptedException) { }
            if (drop == true)
            {
                DropGoodies();
            }
            AddScore();
            Pause.Detach(this);
        }

        public override void Update()
        {
            IsPaused = Pause.GetState();
        }

        protected abstract void FollowPlayer();

        protected abstract void DropGoodies();

        protected abstract void AddScore();
    }
}
