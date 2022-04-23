using System.Threading;

namespace Left4Dead
{
    public class HunterZombie : Zombie
    {
        private Thread jumpThread;

        public int JumpRange { get; set; }

        public HunterZombie(Pause pause) : base(pause)
        {
            Health = 30;
            MaxHealth = 30;
            WalkSpeed = 250;
            AttackDamage = 15;
            AttackSpeed = 1000;
            Direction = Direction.DOWN;
            JumpRange = 10;
            IsPushable = true;
            jumpThread = null;
            ZombieModel = new HunterZombieModel(GameData.HunterZombieModel);
            ZombiePosition = FindSpawnPosition();
            ZombieState = new NormalState();
            GameData.AddZombieAtPosition(ZombiePosition, this);
            Thread zombieThread = new Thread(() => LiveTemplateMethod());
            GameData.AddThread(zombieThread);
            this.ZombieThread = zombieThread;
            zombieThread.Start();
        }

        private bool isPlayerAhead()
        {
            lock (GameData.ConsoleAccessObject)
            {
                switch (Direction)
                {
                    case Direction.UP:
                        if (GameData.Player.Position.X == ZombiePosition.X && ZombiePosition.Y - GameData.Player.Position.Y <= JumpRange)
                        {
                            if (GameData.CheckForCollision(new Position(ZombiePosition.X, ZombiePosition.Y - 1)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                    case Direction.RIGHT:
                        if (GameData.Player.Position.Y == ZombiePosition.Y && GameData.Player.Position.X - ZombiePosition.X <= JumpRange)
                        {
                            if (GameData.CheckForCollision(new Position(ZombiePosition.X + 1, ZombiePosition.Y)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                    case Direction.DOWN:
                        if (GameData.Player.Position.X == ZombiePosition.X && GameData.Player.Position.Y - ZombiePosition.Y <= JumpRange)
                        {
                            if (GameData.CheckForCollision(new Position(ZombiePosition.X, ZombiePosition.Y + 1)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                    case Direction.LEFT:
                        if (GameData.Player.Position.Y == ZombiePosition.Y && ZombiePosition.X - GameData.Player.Position.X <= JumpRange)
                        {
                            if (GameData.CheckForCollision(new Position(ZombiePosition.X - 1, ZombiePosition.Y)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                }
            }
            return false;
        }

        public override void Kill()
        {
            if (jumpThread != null)
            {
                jumpThread.Interrupt();
            }
            jumpThread = null;
            ZombieThread.Interrupt();
            GameData.RemoveThread(jumpThread);
            GameData.RemoveThread(ZombieThread);
            GameData.ZombieAtPosition.Remove(ZombiePosition);
            GameData.ClearPosition(ZombiePosition);
        }

        public override void MakeUpset()
        {
            ZombieState = new AngryState();
            WalkSpeed = 175;
            AttackDamage = 30;
            JumpRange = 20;
            IsPushable = false;
        }

        protected override void AddScore()
        {
            lock (GameData.PlayerScoreAccessObject)
            {
                GameData.Player.Score += 5;
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

            if (weaponOrPack > 50)
            {
                if (number <= 65 && number > 30)
                {
                    Drop drop = new HealthBonus(new AmmoBonus(new AmmoPack()));
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 30)
                {
                    Drop drop = new AmmoBonus(new HealthBonus(new HealthPack()));
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
            else
            {
                if (number <= 50 && number > 25)
                {
                    Drop drop = new AK47Drop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(Pause).TrackDrop(ZombiePosition, drop));
                    GameData.AddThread(dropTrackingThread);
                    drop.SetTrackingThread(dropTrackingThread);
                    GameData.AddDropAtPosition(ZombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 25)
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
            else if (isPlayerAhead())
            {
                lock (GameData.JumpThreadAccess)
                {
                    jumpThread = new Thread(() => new JumpTracker(Pause).Jump(Direction, this));
                    jumpThread.Start();
                    GameData.AddThread(jumpThread);
                }
                GameData.JumpWait.WaitOne();
                Thread.Sleep(1000);
            }
            else
            {
                ZombieState.FindNextStep(this);
            }
        }
    }
}
