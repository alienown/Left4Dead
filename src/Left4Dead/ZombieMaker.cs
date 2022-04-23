using System.Threading;

namespace Left4Dead
{
    public class ZombieMaker : PauseObserver
    {
        private IDifficulty difficulty;
        private Zombie zombie;

        public int ZombieCount { get; set; }
        public int SleepInterval { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerScore { get; set; }
        public double GameTime { get; set; }
        public string WhichZombie { get; set; }

        public ZombieMaker(IDifficulty difficulty, Pause pause)
        {
            this.difficulty = difficulty;
            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
            GameTime = 1;
        }

        public void MakeZombies()
        {
            try
            {
                while (true)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                    lock (GameData.ZombieAtPositionDictionaryAccess)
                    {
                        ZombieCount = GameData.ZombieAtPosition.Count;
                    }
                    lock (GameData.PlayerScoreAccessObject)
                    {
                        PlayerScore = GameData.Player.Score;
                    }
                    lock (GameData.PlayerHealthAccess)
                    {
                        PlayerHealth = GameData.Player.Health;
                    }

                    difficulty.PlanNextSpawn(this);

                    switch (WhichZombie)
                    {
                        case "NormalZombie":
                            zombie = new NormalZombie(Pause);
                            break;
                        case "TankZombie":
                            zombie = new TankZombie(Pause);
                            break;
                        case "HunterZombie":
                            zombie = new HunterZombie(Pause);
                            break;
                        case "None":
                            break;
                    }

                    GameTime += SleepInterval / 1000;
                    Thread.Sleep(SleepInterval);
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        public override void Update()
        {
            IsPaused = Pause.GetState();
        }
    }
}
