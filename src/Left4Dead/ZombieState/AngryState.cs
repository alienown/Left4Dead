namespace Left4Dead
{
    public class AngryState : IZombieState
    {
        public void FindNextStep(Zombie z)
        {
            Position zombiePositionCopy = new Position(z.ZombiePosition);
            Shift shift;

            lock (GameData.ConsoleAccessObject)
            {
                shift = GameData.GameMechanics.GetBestZombieShift(z.ZombiePosition, true);

                switch (GameData.CheckForCollision(new Position(z.ZombiePosition.X + shift.Position.X, z.ZombiePosition.Y + shift.Position.Y)))
                {
                    case Collision.NONE:
                        zombiePositionCopy.X += shift.Position.X;
                        zombiePositionCopy.Y += shift.Position.Y;
                        break;
                    case Collision.WALL:
                        break;
                    case Collision.ZOMBIE:
                        if (GameData.GameMechanics.TryToPushZombie(new Position(zombiePositionCopy.X + shift.Position.X, zombiePositionCopy.Y + shift.Position.Y)))
                        {
                            zombiePositionCopy.X += shift.Position.X;
                            zombiePositionCopy.Y += shift.Position.Y;
                        }
                        break;
                    case Collision.DROP:
                        zombiePositionCopy.X += shift.Position.X;
                        zombiePositionCopy.Y += shift.Position.Y;
                        GameData.RemoveDropAtPosition(zombiePositionCopy);
                        break;
                    case Collision.BULLET:
                        break;
                }
                z.Direction = shift.Direction;
                GameData.DisplayModelAtPosition(zombiePositionCopy, z.ZombiePosition, z.ZombieModel, z.Direction, true);
            }
        }
    }
}
