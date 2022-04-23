namespace Left4Dead
{
    public class MediumDifficulty : IDifficulty
    {
        public void PlanNextSpawn(ZombieMaker z)
        {
            if (z.ZombieCount < 10)
            {
                if (z.PlayerScore - GameData.GenerateRandomNumber(5, 50) < 0)
                {
                    z.WhichZombie = "NormalZombie";
                }
                else
                {
                    z.WhichZombie = "TankZombie";
                }

                z.SleepInterval = GameData.GenerateRandomNumber(1000, 2501);
            }
            else
            {
                z.SleepInterval = 0;
                z.WhichZombie = "None";
            }
        }

        public override string ToString()
        {
            return "Medium";
        }
    }
}
