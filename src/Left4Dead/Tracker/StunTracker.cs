using System;
using System.Threading;

namespace Left4Dead
{
    public class StunTracker : PauseObserver
    {
        public StunTracker(Pause pause)
        {
            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
        }

        public void TrackStun()
        {
            int stunTime = 2000;

            try
            {
                while (stunTime > 0)
                {
                    Thread.Sleep(16);
                    stunTime -= 16;
                    lock (GameData.ConsoleAccessObject)
                    {
                        Console.SetCursorPosition(0, 3);
                        Console.Write("    ");
                        Console.Write(stunTime);
                    }
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                }
            }
            catch (ThreadInterruptedException) { }

            lock (GameData.PlayerStunnedAccess)
            {
                GameData.Player.IsStunned = false;
            }
            Pause.Detach(this);
            GameData.RemoveThread(Thread.CurrentThread);
        }
        public override void Update()
        {
            IsPaused = Pause.GetState();
        }
    }
}
