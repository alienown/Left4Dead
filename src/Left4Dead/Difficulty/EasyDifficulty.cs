namespace Left4Dead
{
    public class EasyDifficulty : IDifficulty
    {
        public void PlanNextSpawn(ZombieMaker z)
        {
            if (z.ZombieCount < 10)
            {
                z.WhichZombie = "NormalZombie";
                z.SleepInterval = 5000;
            }
            else
            {
                z.SleepInterval = 0;
                z.WhichZombie = "None";
            }
        }

        public override string ToString()
        {
            return "Easy";
        }
    }
}
