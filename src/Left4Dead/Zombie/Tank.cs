using System.Threading;

namespace Left4Dead
{
    public class TankZombie : Zombie
    {
        public TankZombie(Pause pause) : base(pause)
        {
            Health = 100;
            MaxHealth = 100;
            WalkSpeed = 350;
            AttackDamage = 35;
            AttackSpeed = 2000;
            IsPushable = false;
            Direction = Direction.DOWN;
            ZombieModel = new TankZombieModel(GameData.TankZombieModel);
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
            ZombieState = new EnragedState();
            WalkSpeed = 250;
            AttackDamage = 50;
        }

        protected override void AddScore()
        {
            lock (GameData.PlayerScoreAccessObject)
            {
                GameData.Player.Score += 8;
            }
            GameData.PlayerScoreStatusEvent.Set();
        }

        protected override void DropGoodies()
        {
            int number, weaponOrPack;
            lock (GameData.GenerateRandomNumberAccess)
            {
                weaponOrPack = GameData.GenerateRandomNumber(0, 2);
                number = GameData.GenerateRandomNumber(0, 2);
            }

            if (weaponOrPack == 0)
            {
                if (number == 1)
                {
                    Drop drop = new HealthBonus(new AmmoPack());
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number == 0)
                {
                    Drop drop = new AmmoBonus(new HealthPack());
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
            else
            {
                if (number == 1)
                {
                    Drop drop = new AK47Drop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number == 0)
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

                            lock (GameData.PlayerStunnedAccess)
                            {
                                if (!GameData.Player.IsStunned)
                                {
                                    Thread pushAndStunThread = new Thread(() => new PushTracker(Pause).PushAndStun(Direction));
                                    pushAndStunThread.Start();
                                    GameData.AddThread(pushAndStunThread);
                                }
                            }
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
