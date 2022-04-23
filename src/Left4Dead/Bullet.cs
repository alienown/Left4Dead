using System;
using System.Threading;

namespace Left4Dead
{
    public class Bullet : PauseObserver
    {
        private int range;
        private Direction direction;
        private Position bulletPosition;
        private Model bulletModel;
        private bool isDone;
        private bool clearPrevious;

        public Bullet(Position playerPosition, Direction direction, Model model, int range, Pause pause)
        {
            isDone = false;
            clearPrevious = false;
            bulletPosition = new Position(playerPosition);
            this.direction = direction;
            bulletModel = model;
            this.range = range;

            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
        }

        public void Fly()
        {
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

            while (!isDone && range > 0)
            {
                if (IsPaused)
                {
                    GameData.PauseGameEvent.Reset();
                    GameData.PauseGameEvent.WaitOne();
                }

                lock (GameData.ConsoleAccessObject)
                {
                    Position bulletPositionCopy = new Position(bulletPosition);
                    switch (GameData.CheckForCollision(new Position(bulletPosition.X + verticalShift, bulletPosition.Y + horizontalShift)))
                    {
                        case Collision.NONE:
                            bulletPositionCopy.X += verticalShift;
                            bulletPositionCopy.Y += horizontalShift;
                            GameData.DisplayModelAtPosition(bulletPositionCopy, bulletPosition, bulletModel, direction, clearPrevious);
                            break;
                        case Collision.WALL:
                            if (!GameData.Player.Position.Equals(bulletPosition))
                                GameData.ClearPosition(bulletPosition);
                            isDone = true;
                            break;
                        case Collision.ZOMBIE:
                            bulletPositionCopy.X += verticalShift;
                            bulletPositionCopy.Y += horizontalShift;
                            GameData.GameMechanics.HitZombie(bulletPositionCopy, GameData.Player.CurrentWeapon.Damage);
                            if (clearPrevious)
                                GameData.ClearPosition(bulletPosition);
                            isDone = true;
                            break;
                        case Collision.DROP:
                            bulletPositionCopy.X += verticalShift;
                            bulletPositionCopy.Y += horizontalShift;
                            GameData.ClearPosition(bulletPosition);
                            GameData.RemoveDropAtPosition(bulletPositionCopy);
                            isDone = true;
                            break;
                    }

                    if (range == 1)
                    {
                        GameData.ClearPosition(bulletPosition);
                    }
                }

                range -= 1;
                clearPrevious = true;
                Thread.Sleep(GameData.Player.CurrentWeapon.BulletSpeed);
            }
            Pause.Detach(this);
        }

        public override void Update()
        {
            IsPaused = Pause.GetState();
        }
    }
}
