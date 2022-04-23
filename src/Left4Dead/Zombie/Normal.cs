using System.Threading;

namespace Left4Dead
{
    public class NormalZombie : Zombie
    {
        public NormalZombie(Pause pause) : base(pause)
        {
            Health = 10;
            MaxHealth = 10;
            WalkSpeed = 1000;
            AttackDamage = 10;
            AttackSpeed = 1000;
            IsPushable = true;
            Direction = Direction.DOWN;
            ZombieModel = new NormalZombieModel(GameData.NormalZombieModel);
            ZombiePosition = FindSpawnPosition();
            ZombieState = new NormalState();
            GameData.AddZombieAtPosition(ZombiePosition, this);
            Thread zombieThread = new Thread(() => LiveTemplateMethod());
            GameData.AddThread(zombieThread);
            this.ZombieThread = zombieThread;
            zombieThread.Start();
        }

        public override void Kill()
        {
            ZombieThread.Interrupt();
            GameData.RemoveThread(ZombieThread);
            GameData.ZombieAtPosition.Remove(ZombiePosition);
            GameData.ClearPosition(ZombiePosition);
        }

        public override void MakeUpset()
        {
            ZombieState = new AngryState();
            WalkSpeed = 500;
            AttackDamage = 20;
            IsPushable = false;
        }

        protected override void AddScore()
        {
            lock (GameData.PlayerScoreAccessObject)
            {
                GameData.Player.Score += 1;
            }
            GameData.PlayerScoreStatusEvent.Set();
        }

        protected override void DropGoodies()
        {
            int number, weaponOrPack = -1;
            lock (GameData.GenerateRandomNumberAccess)
            {
                weaponOrPack = GameData.GenerateRandomNumber(0, 100);
                number = GameData.GenerateRandomNumber(0, 100);
            }

            if (weaponOrPack > 20)
            {
                if (number <= 35 && number > 10)
                {
                    Drop drop = new AmmoPack();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 10)
                {
                    Drop drop = new HealthPack();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
            else
            {
                if (number <= 35 && number > 10)
                {
                    Drop drop = new AK47Drop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 10)
                {
                    Drop drop = new SniperRifleDrop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
        }

        protected override void FollowPlayer()
        {
            if (IsPlayerAround())
            {
                do
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                    lock (GameData.ConsoleAccessObject)
                    {
                        if (GameData.Player.Health > 0)
                        {
                            GameData.GameMechanics.HitPlayer(AttackDamage);
                            GameData.PlayerHealthStatusEvent.Set();
                        }
                    }
                    Thread.Sleep(AttackSpeed);
                } while (IsPlayerAround());
            }
            else
            {
                ZombieState.FindNextStep(this);
            }
        }
    }
}
