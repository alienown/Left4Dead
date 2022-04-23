namespace Left4Dead
{
    public class HardDifficulty : IDifficulty
    {
        public void PlanNextSpawn(ZombieMaker z)
        {
            int interval;

            double scorePerMinute = (z.PlayerScore * 60) / z.GameTime;

            if (z.ZombieCount < 10)
            {
                if (scorePerMinute <= 20)
                {
                    z.WhichZombie = "NormalZombie";
                }
                else if (scorePerMinute <= GameData.GenerateRandomNumber(0, z.PlayerHealth))
                {
                    z.WhichZombie = "TankZombie";
                }
                else
                {
                    z.WhichZombie = "HunterZombie";
                }

                interval = (GameData.GenerateRandomNumber(1000, 2501) - z.PlayerScore * 10);

                if (interval < 0)
                {
                    z.SleepInterval = 0;
                }
                else
                {
                    z.SleepInterval = interval;
                }
            }
            else
            {
                z.SleepInterval = 0;
                z.WhichZombie = "None";
            }
        }

        public override string ToString()
        {
            return "Hard";
        }
    }
}
