using System;
using System.Threading;

namespace Left4Dead
{
    public class JumpTracker : PauseObserver
    {
        public JumpTracker(Pause pause)
        {
            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
        }

        public void Jump(Direction direction, HunterZombie zombie)
        {
            bool isDone = false;
            int jumpRange = zombie.JumpRange;
            Position zombiePositionCopy = new Position(zombie.ZombiePosition);
            int verticalShift = 0, horizontalShift = 0;

            switch (direction)
            {
                case Direction.UP:
                    verticalShift = 0;
                    horizontalShift = -1;
                    break;
                case Direction.RIGHT:
                    verticalShift = 1;
                    horizontalShift = 0;
                    break;
                case Direction.DOWN:
                    verticalShift = 0;
                    horizontalShift = 1;
                    break;
                case Direction.LEFT:
                    verticalShift = -1;
                    horizontalShift = 0;
                    break;
            }

            try
            {
                while (!isDone && jumpRange > 0)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }

                    lock (GameData.ConsoleAccessObject)
                    {
                        switch (GameData.CheckForCollision(new Position(zombie.ZombiePosition.X + verticalShift, zombie.ZombiePosition.Y + horizontalShift)))
                        {
                            case Collision.NONE:
                                zombiePositionCopy.Y += horizontalShift;
                                zombiePositionCopy.X += verticalShift;
                                jumpRange -= 1;
                                break;
                            case Collision.PLAYER:
                                lock (GameData.PlayerHealthAccess)
                                {
                                    GameData.GameMechanics.HitPlayer(zombie.AttackDamage);
                                    GameData.PlayerHealthStatusEvent.Set();
                                }
                                isDone = true;
                                break;
                            case Collision.WALL:
                                isDone = true;
                                break;
                            case Collision.ZOMBIE:
                                isDone = true;
                                break;
                            case Collision.DROP:
                                zombiePositionCopy.Y += horizontalShift;
                                zombiePositionCopy.X += verticalShift;
                                jumpRange -= 1;
                                GameData.RemoveDropAtPosition(zombiePositionCopy);
                                break;
                        }
                        GameData.DisplayModelAtPosition(zombiePositionCopy, zombie.ZombiePosition, zombie.ZombieModel, direction, true);
                    }
                    Thread.Sleep(50);
                }
            }
            catch (ThreadInterruptedException)
            {

            }
            GameData.JumpWait.Set();
            Pause.Detach(this);
        }
        public override void Update()
        {
            IsPaused = Pause.GetState();
        }
    }
}
