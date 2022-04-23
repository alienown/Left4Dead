using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Left4Dead
{
    public class Mechanics : PauseObserver
    {
        public Mechanics(Pause pause)
        {
            pause.Attach(this);
            this.Pause = pause;
            IsPaused = pause.GetState();
        }

        private void GrabDrop(Position position)
        {
            Drop drop;
            drop = GameData.DropAtPosition[GameData.DropAtPosition.Keys.Where(d => d.Equals(position)).First()];
            drop.AddValue();
        }

        private void Shoot(Model model, int range)
        {
            try
            {
                Bullet bullet = new Bullet(GameData.Player.Position, GameData.Player.Direction, model, range, Pause);
                bullet.Fly();
            }
            catch (ThreadInterruptedException)
            {
                GameData.RemoveThread(Thread.CurrentThread);
            }
        }

        private void PerformAction(ConsoleKeyInfo keyInfo)
        {
            Position playerPositionCopy;
            bool wait = false;

            lock (GameData.PlayerStunnedAccess)
            {
                if (GameData.Player.IsStunned)
                    wait = true;
            }

            if (wait == true)
            {
            }
            else if (IsPaused)
            {
                GameData.PauseGameEvent.Reset();
                GameData.PauseGameEvent.WaitOne();
            }
            else
            {
                playerPositionCopy = new Position(GameData.Player.Position);

                lock (GameData.ConsoleAccessObject)
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.RightArrow:
                            switch (GameData.CheckForCollision(new Position(GameData.Player.Position.X + 1, GameData.Player.Position.Y)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.X++;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.X++;
                                    GrabDrop(playerPositionCopy);
                                    GameData.RemoveDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.Player.Direction = Direction.RIGHT;
                            GameData.DisplayModelAtPosition(playerPositionCopy, GameData.Player.Position, GameData.Player.PlayerModel, Direction.RIGHT, true);
                            break;

                        case ConsoleKey.LeftArrow:
                            switch (GameData.CheckForCollision(new Position(GameData.Player.Position.X - 1, GameData.Player.Position.Y)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.X--;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.X--;
                                    GrabDrop(playerPositionCopy);
                                    GameData.RemoveDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.Player.Direction = Direction.LEFT;
                            GameData.DisplayModelAtPosition(playerPositionCopy, GameData.Player.Position, GameData.Player.PlayerModel, Direction.LEFT, true);
                            break;

                        case ConsoleKey.UpArrow:
                            switch (GameData.CheckForCollision(new Position(GameData.Player.Position.X, GameData.Player.Position.Y - 1)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.Y--;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.Y--;
                                    GrabDrop(playerPositionCopy);
                                    GameData.RemoveDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.Player.Direction = Direction.UP;
                            GameData.DisplayModelAtPosition(playerPositionCopy, GameData.Player.Position, GameData.Player.PlayerModel, Direction.UP, true);
                            break;

                        case ConsoleKey.DownArrow:
                            switch (GameData.CheckForCollision(new Position(GameData.Player.Position.X, GameData.Player.Position.Y + 1)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.Y++;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.Y++;
                                    GrabDrop(playerPositionCopy);
                                    GameData.RemoveDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.Player.Direction = Direction.DOWN;
                            GameData.DisplayModelAtPosition(playerPositionCopy, GameData.Player.Position, GameData.Player.PlayerModel, Direction.DOWN, true);
                            break;

                        case ConsoleKey.Spacebar:
                            lock (GameData.WeaponAccessObject)
                            {
                                if (GameData.Player.CurrentWeapon.Ammo == -1)
                                {
                                    if (GameData.Player.IsReloaded == true)
                                    {
                                        Thread InstanceCaller = new Thread(() => Shoot(GameData.Player.CurrentWeapon.BulletModel, GameData.Player.CurrentWeapon.Range));
                                        GameData.AddThread(InstanceCaller);
                                        InstanceCaller.Start();
                                        GameData.Player.IsReloaded = false;
                                        GameData.ReloadEvent.Set();
                                    }
                                }
                                else if (GameData.Player.CurrentWeapon.Ammo > 0)
                                {
                                    if (GameData.Player.IsReloaded == true)
                                    {
                                        Thread InstanceCaller = new Thread(() => Shoot(GameData.Player.CurrentWeapon.BulletModel, GameData.Player.CurrentWeapon.Range));
                                        GameData.AddThread(InstanceCaller);
                                        InstanceCaller.Start();
                                        GameData.Player.CurrentWeapon.Ammo -= 1;
                                        GameData.Player.IsReloaded = false;
                                        GameData.PlayerAmmoStatusEvent.Set();
                                        GameData.ReloadEvent.Set();
                                    }
                                }
                            }
                            break;
                        case ConsoleKey.D1:
                            lock (GameData.WeaponAccessObject)
                            {
                                Weapon weapon = GameData.Player.Weapons.Find(w => w.ToString() == "Pistol");
                                if (!(weapon == null) && GameData.Player.CurrentWeapon != weapon)
                                {
                                    GameData.Player.CurrentWeapon = weapon;
                                    GameData.PlayerAmmoStatusEvent.Set();
                                    GameData.PlayerCurrentWeaponStatusEvent.Set();
                                }
                            }
                            break;
                        case ConsoleKey.D2:
                            lock (GameData.WeaponAccessObject)
                            {
                                Weapon weapon = GameData.Player.Weapons.Find(w => w.ToString() == "AK47");
                                if (!(weapon == null) && GameData.Player.CurrentWeapon != weapon)
                                {
                                    GameData.Player.CurrentWeapon = weapon;
                                    GameData.PlayerAmmoStatusEvent.Set();
                                    GameData.PlayerCurrentWeaponStatusEvent.Set();
                                }
                            }
                            break;
                        case ConsoleKey.D3:
                            lock (GameData.WeaponAccessObject)
                            {
                                Weapon weapon = GameData.Player.Weapons.Find(w => w.ToString() == "Sniper Rifle");
                                if (!(weapon == null) && GameData.Player.CurrentWeapon != weapon)
                                {
                                    GameData.Player.CurrentWeapon = weapon;
                                    GameData.PlayerAmmoStatusEvent.Set();
                                    GameData.PlayerCurrentWeaponStatusEvent.Set();
                                }
                            }
                            break;
                    }
                }
            }
        }

        public bool TryToPushZombie(Position position)
        {
            Zombie zombie;

            lock (GameData.ZombieAtPositionDictionaryAccess)
            {
                position = GameData.ZombieAtPosition.Keys.Where(p => p.Equals(position)).FirstOrDefault();
                zombie = GameData.ZombieAtPosition[position];
            }

            int verticalShift = GameData.GenerateRandomNumber(-1, 2);
            int horizontalShift = GameData.GenerateRandomNumber(-1, 2);

            Position nextPosition = new Position(position.X + verticalShift, position.Y + horizontalShift);

            if (GameData.CheckForCollision(nextPosition) == (int)Collision.NONE)
            {
                if (zombie.IsPushable)
                {
                    GameData.DisplayModelAtPosition(nextPosition, position, zombie.ZombieModel, zombie.Direction);
                    return true;
                }
            }

            return false;
        }

        public Shift GetBestZombieShift(Position zombiePosition, bool possibleDiagonal = false)
        {
            Shift shift = new Shift();
            Position playerPosition = GameData.Player.Position;

            if (possibleDiagonal == false)
            {
                if (Math.Abs(playerPosition.X - zombiePosition.X) > Math.Abs(playerPosition.Y - zombiePosition.Y))
                {
                    if (playerPosition.X - zombiePosition.X > 0)
                    {
                        shift.Position = new Position(1, 0);
                        shift.Direction = Direction.RIGHT;
                    }
                    else
                    {
                        shift.Position = new Position(-1, 0);
                        shift.Direction = Direction.LEFT;
                    }
                }
                else
                {
                    if (playerPosition.Y - zombiePosition.Y > 0)
                    {
                        shift.Position = new Position(0, 1);
                        shift.Direction = Direction.DOWN;
                    }
                    else
                    {
                        shift.Position = new Position(0, -1);
                        shift.Direction = Direction.UP;
                    }
                }
            }
            else
            {
                if (playerPosition.X == zombiePosition.X)
                {
                    if (playerPosition.Y - zombiePosition.Y > 0)
                    {
                        shift.Position = new Position(0, 1);
                        shift.Direction = Direction.DOWN;
                    }
                    else
                    {
                        shift.Position = new Position(0, -1);
                        shift.Direction = Direction.UP;
                    }
                }
                else if (playerPosition.Y == zombiePosition.Y)
                {
                    if (playerPosition.X - zombiePosition.X > 0)
                    {
                        shift.Position = new Position(1, 0);
                        shift.Direction = Direction.RIGHT;
                    }
                    else
                    {
                        shift.Position = new Position(-1, 0);
                        shift.Direction = Direction.LEFT;
                    }
                }
                else
                {
                    if (playerPosition.X > zombiePosition.X && playerPosition.Y < zombiePosition.Y)
                    {
                        shift.Position = new Position(1, -1);
                        shift.Direction = Direction.UP;
                    }
                    else if (playerPosition.X < zombiePosition.X && playerPosition.Y < zombiePosition.Y)
                    {
                        shift.Position = new Position(-1, -1);
                        shift.Direction = Direction.UP;
                    }
                    else if (playerPosition.X > zombiePosition.X && playerPosition.Y > zombiePosition.Y)
                    {
                        shift.Position = new Position(1, 1);
                        shift.Direction = Direction.DOWN;
                    }
                    else if (playerPosition.X < zombiePosition.X && playerPosition.Y > zombiePosition.Y)
                    {
                        shift.Position = new Position(-1, 1);
                        shift.Direction = Direction.DOWN;
                    }
                }
            }

            return shift;
        }

        public ConsoleColor GetFontColourBasedOnHealth(int health, int maxHealth, Object lockObject)
        {
            lock (lockObject)
            {
                if (health >= 0.75 * maxHealth)
                {
                    return ConsoleColor.Green;
                }
                else if (health >= 0.5 * maxHealth)
                {
                    return ConsoleColor.Yellow;
                }
                else if (health >= 0.25 * maxHealth)
                {
                    return ConsoleColor.DarkYellow;
                }
                else
                {
                    return ConsoleColor.Red;
                }
            }
        }

        public void ReloadWeapon()
        {
            int reloadSpeed;

            try
            {
                while (true)
                {
                    GameData.ReloadEvent.Reset();
                    GameData.ReloadEvent.WaitOne();

                    lock (GameData.WeaponAccessObject)
                    {
                        reloadSpeed = GameData.Player.CurrentWeapon.ReloadSpeed;
                    }


                    while (reloadSpeed > 0)
                    {
                        Thread.Sleep(16);
                        reloadSpeed -= 16;
                        lock (GameData.ConsoleAccessObject)
                        {
                            Console.SetCursorPosition(0, 0);
                            Console.Write("    ");
                            Console.Write(reloadSpeed);
                        }
                        if (IsPaused)
                        {
                            GameData.PauseGameEvent.Reset();
                            GameData.PauseGameEvent.WaitOne();
                        }
                    }

                    lock (GameData.WeaponAccessObject)
                    {
                        GameData.Player.IsReloaded = true;
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        public bool HitZombie(Position position, int damage, bool drops = true)
        {
            Zombie zombie;
            lock (GameData.ConsoleAccessObject)
            {
                lock (GameData.ZombieAtPositionDictionaryAccess)
                {
                    position = GameData.ZombieAtPosition.Keys.Where(p => p.Equals(position)).FirstOrDefault();
                    zombie = GameData.ZombieAtPosition[position];
                }
                lock (GameData.ZombieHealthAccess)
                {
                    zombie.Health -= damage;
                    zombie.ZombieModel.ModelColor = GetFontColourBasedOnHealth(zombie.Health, zombie.MaxHealth, GameData.ZombieHealthAccess);
                    GameData.DisplayModelAtPosition(position, position, zombie.ZombieModel, zombie.Direction);
                    if (zombie.Health <= 0)
                    {
                        zombie.SetDrop(drops);
                        GameData.RemoveZombieAtPosition(position);
                        return true;
                    }
                }
                return false;
            }
        }

        public void HitPlayer(int damage)
        {
            lock (GameData.PlayerHealthAccess)
            {
                if (GameData.Player.Health - damage >= 0)
                {
                    GameData.Player.Health -= damage;
                    GameData.PlayerHealthStatusEvent.Set();
                }
                else
                {
                    GameData.Player.Health = 0;
                }
            }
        }

        public void GameOver()
        {
            Pause.SetState();
            Menus.GameOverMenu();
            if (GameData.Option == "TRY AGAIN")
            {
                GameData.RemoveAllThreads();
                GameData.Player.Dispose();
                GameData.InitializeGame();
            }
            else if (GameData.Option == "EXIT TO MAIN MENU")
            {
                ExitGame();
            }
        }

        public void PauseGame()
        {
            Pause.SetState();
            Menus.PauseMenu();
        }

        public void ResumeGame()
        {
            Pause.SetState();
        }

        public static void ExitGame()
        {
            Console.BackgroundColor = GameData.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            lock (GameData.ConsoleAccessObject)
            {
                if (GameData.Player.Health <= 0)
                {
                    Console.SetCursorPosition(53, 27);
                }
                else
                {
                    Console.SetCursorPosition(105, 30);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Quitting to Main Menu...");
            }
            Mechanics.SaveScore();
            GameData.RemoveAllThreads();
            Thread.Sleep(1000);
            GameData.Player.Dispose();
            GameData.ExitGame.Set();
        }

        public static void SaveScore()
        {
            string line;
            List<string> words = new List<string>();

            try
            {
                StreamReader file = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ranking.txt");
                while ((line = file.ReadLine()) != null)
                {
                    words.Add(line);

                }
                words.Add(GameData.Player.Name + " " + GameData.Player.Score + " " + GameData.Difficulty);
                file.Close();
            }
            catch (FileNotFoundException)
            {
                words.Add(GameData.Player.Name + " " + GameData.Player.Score + " " + GameData.Difficulty);
            }

            StreamWriter writer = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ranking.txt", false);

            foreach (var word in words)
            {
                writer.WriteLine(word);
            }
            writer.Close();
        }

        public void StartGame()
        {
            bool dead = false;

            try
            {
                while (true)
                {
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }
                    GameData.KeyInfo = Console.ReadKey(true);
                    lock (GameData.PlayerHealthAccess)
                    {
                        if (GameData.Player.Health <= 0)
                        {
                            dead = true;
                        }
                    }
                    if (dead == true)
                    {
                        GameData.KeyInfoWaitEvent.Set();
                        if (GameData.KeyInfo.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (GameData.KeyInfo.Key == ConsoleKey.Escape)
                        {
                            PauseGame();
                            if (GameData.Option == "RESUME")
                            {
                                Console.SetCursorPosition(105, 26);
                                Console.Write("                        ");
                                Console.SetCursorPosition(105, 28);
                                Console.Write("                        ");
                                ResumeGame();
                            }
                            else if (GameData.Option == "EXIT TO MAIN MENU")
                            {
                                ExitGame();
                                break;
                            }
                        }
                        else
                        {


                            PerformAction(GameData.KeyInfo);
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
