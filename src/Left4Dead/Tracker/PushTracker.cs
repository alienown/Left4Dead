using System;
using System.Threading;

namespace Left4Dead
{
    public class PushTracker : PauseObserver
    {
        public PushTracker(Pause pause)
        {
            lock (GameData.PlayerStunnedAccess)
            {
                GameData.Player.IsStunned = true;
            }
            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
        }

        public void PushAndStun(Direction direction)
        {
            Position playerPositionCopy = new Position(GameData.Player.Position);
            bool isDone = false;

            GameData.Player.Direction = direction;

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
                while (!isDone)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }

                    lock (GameData.ConsoleAccessObject)
                    {
                        switch (GameData.CheckForCollision(new Position(GameData.Player.Position.X + verticalShift, GameData.Player.Position.Y + horizontalShift)))
                        {
                            case Collision.NONE:
                                playerPositionCopy.Y += horizontalShift;
                                playerPositionCopy.X += verticalShift;
                                break;
                            case Collision.WALL:
                                lock (GameData.PlayerHealthAccess)
                                {
                                    if (GameData.Player.Health > 0)
                                    {
                                        GameData.Player.Health = GameData.Player.Health - 5;
                                        GameData.PlayerHealthStatusEvent.Set();
                                    }
                                }
                                isDone = true;
                                break;
                            case Collision.ZOMBIE:
                                lock (GameData.PlayerHealthAccess)
                                {
                                    if (GameData.Player.Health > 0)
                                    {
                                        GameData.Player.Health = GameData.Player.Health - 5;
                                        GameData.PlayerHealthStatusEvent.Set();
                                    }
                                }
                                isDone = true;
                                break;
                            case Collision.DROP:
                                playerPositionCopy.Y += horizontalShift;
                                playerPositionCopy.X += verticalShift;
                                GameData.RemoveDropAtPosition(playerPositionCopy);
                                break;
                        }

                        GameData.DisplayModelAtPosition(playerPositionCopy, GameData.Player.Position, GameData.Player.PlayerModel, GameData.Player.Direction, true);
                    }

                    Thread.Sleep(50);
                }

                Thread stunTrackingThread;
                stunTrackingThread = new Thread(() => new StunTracker(Pause).TrackStun());
                stunTrackingThread.Start();
                GameData.AddThread(stunTrackingThread);
                Pause.Detach(this);
            }
            catch (ThreadInterruptedException) { }
        }
        public override void Update()
        {
            IsPaused = Pause.GetState();
        }
    }
}
