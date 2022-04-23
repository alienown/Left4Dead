using System;
using System.Threading;

namespace Left4Dead
{
    public class DropTracker : PauseObserver
    {
        public DropTracker(Pause pause)
        {
            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
        }

        public void TrackDrop(Position position, Drop drop)
        {
            int appearTime = drop.GetAppearTime();
            try
            {
                int pom = 1;
                while (appearTime > 0)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }

                    Thread.Sleep(1000);
                    appearTime -= 1000;
                    if (appearTime <= 4000)
                    {
                        if (pom == 1)
                        {
                            drop.GetModel().ModelColor = ConsoleColor.White;
                            pom = 0;
                        }
                        else
                        {
                            drop.GetModel().ModelColor = drop.GetModel().ModelBaseColor;
                            pom = 1;
                        }
                        GameData.DisplayModelAtPosition(position, position, drop.GetModel(), 0, false);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {

            }
            GameData.RemoveDropAtPosition(position);
            Pause.Detach(this);
        }

        public override void Update()
        {
            IsPaused = Pause.GetState();
        }
    }
}
