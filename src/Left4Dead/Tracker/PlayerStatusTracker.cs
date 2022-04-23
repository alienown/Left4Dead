using System;
using System.Threading;

namespace Left4Dead
{
    public class PlayerStatusTracker : PauseObserver
    {
        public PlayerStatusTracker(Pause pause)
        {
            pause.Attach(this);
            this.Pause = pause;
            this.IsPaused = pause.GetState();
        }

        public void TrackPlayerHealthStatus()
        {
            try
            {
                lock (GameData.ConsoleAccessObject)
                {
                    Console.SetCursorPosition(105, 6);
                    lock (GameData.PlayerHealthAccess)
                    {
                        Console.Write("Health: ");
                        Console.ForegroundColor = GameData.GameMechanics.GetFontColourBasedOnHealth(GameData.Player.Health, GameData.Player.MaxHealth, GameData.PlayerHealthAccess);
                        Console.Write(GameData.Player.Health);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                while (true)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                    GameData.PlayerHealthStatusEvent.Reset();
                    GameData.PlayerHealthStatusEvent.WaitOne();
                    lock (GameData.ConsoleAccessObject)
                    {
                        GameData.Player.PlayerModel.ModelColor = GameData.GameMechanics.GetFontColourBasedOnHealth(GameData.Player.Health, GameData.Player.MaxHealth, GameData.PlayerHealthAccess);
                        GameData.DisplayModelAtPosition(GameData.Player.Position, GameData.Player.Position, GameData.Player.PlayerModel, GameData.Player.Direction, true);
                        Console.SetCursorPosition(113, 6);
                        lock (GameData.PlayerHealthAccess)
                        {
                            Console.Write("   ");
                            Console.ForegroundColor = GameData.GameMechanics.GetFontColourBasedOnHealth(GameData.Player.Health, GameData.Player.MaxHealth, GameData.PlayerHealthAccess);
                            Console.SetCursorPosition(113, 6);
                            Console.Write(GameData.Player.Health);
                            Console.ForegroundColor = ConsoleColor.White;

                            if (GameData.Player.Health <= 0)
                            {
                                Thread endingThread = new Thread(() => GameData.GameMechanics.GameOver());
                                endingThread.Start();
                                break;
                            }
                        }
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        public void TrackPlayerAmmoStatus()
        {
            try
            {
                lock (GameData.ConsoleAccessObject)
                {
                    Console.SetCursorPosition(105, 14);
                    lock (GameData.WeaponAccessObject)
                    {
                        Console.Write("Ammo: " + GameData.Player.CurrentWeapon.ShowCurrentAmmo());
                    }
                }
                while (true)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                    GameData.PlayerAmmoStatusEvent.Reset();
                    GameData.PlayerAmmoStatusEvent.WaitOne();
                    lock (GameData.ConsoleAccessObject)
                    {
                        Console.SetCursorPosition(111, 14);
                        lock (GameData.WeaponAccessObject)
                        {
                            Console.Write("         ");
                            Console.SetCursorPosition(111, 14);
                            Console.Write(GameData.Player.CurrentWeapon.ShowCurrentAmmo());
                        }
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        public void TrackPlayerCurrentWeaponStatus()
        {
            try
            {
                lock (GameData.ConsoleAccessObject)
                {
                    Console.SetCursorPosition(105, 10);
                    lock (GameData.WeaponAccessObject)
                    {
                        Console.Write("Weapon: " + GameData.Player.CurrentWeapon.ToString());
                    }
                }
                while (true)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                    GameData.PlayerCurrentWeaponStatusEvent.Reset();
                    GameData.PlayerCurrentWeaponStatusEvent.WaitOne();
                    lock (GameData.ConsoleAccessObject)
                    {
                        Console.SetCursorPosition(113, 10);
                        lock (GameData.WeaponAccessObject)
                        {
                            Console.Write("            ");
                            Console.SetCursorPosition(113, 10);
                            Console.Write(GameData.Player.CurrentWeapon.ToString());
                        }
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        public void TrackPlayerScoreStatus()
        {
            try
            {
                lock (GameData.ConsoleAccessObject)
                {
                    Console.SetCursorPosition(105, 18);
                    lock (GameData.WeaponAccessObject)
                    {
                        Console.Write("Score: " + GameData.Player.Score);
                    }
                }
                while (true)
                {
                    if (IsPaused)
                    {
                        GameData.PauseGameEvent.Reset();
                        GameData.PauseGameEvent.WaitOne();
                    }
                    GameData.PlayerScoreStatusEvent.Reset();
                    GameData.PlayerScoreStatusEvent.WaitOne();
                    lock (GameData.ConsoleAccessObject)
                    {
                        Console.SetCursorPosition(112, 18);
                        lock (GameData.WeaponAccessObject)
                        {
                            Console.Write("            ");
                            Console.SetCursorPosition(112, 18);
                            Console.Write(GameData.Player.Score);
                        }
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        public override void Update()
        {
            IsPaused = Pause.GetState();
        }
    }
}
