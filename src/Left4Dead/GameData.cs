using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Left4Dead
{
    public static class GameData
    {
        private static Pause pause;
        private static List<Thread> threadList;
        private static char[,] gameMap;

        public static string Difficulty { get; set; } = "";
        public static ConsoleKeyInfo KeyInfo { get; set; }
        public static Mechanics GameMechanics { get; set; }
        public static Player Player { get; set; }
        public static Dictionary<Position, Zombie> ZombieAtPosition { get; set; }
        public static Dictionary<Position, Drop> DropAtPosition { get; set; }
        public static string InterfaceBuilder { get; set; } = "";
        public static ConsoleColor BackgroundColor { get; set; }
        public static int DropAppearTime = 10000;
        public static string Option;
        public static string PlayerName;

        public static Object JumpThreadAccess { get; set; } = new Object();
        public static Object PlayerScoreAccessObject { get; set; } = new Object();
        public static Object IsPausedAccessObject { get; set; } = new Object();
        public static Object ConsoleAccessObject { get; set; } = new Object();
        public static Object DropAtPositionDictionaryAccess { get; set; } = new Object();
        public static Object PlayerHealthAccess { get; set; } = new Object();
        public static Object ZombieAtPositionDictionaryAccess { get; set; } = new Object();
        public static Object ReloadAccessObject { get; set; } = new Object();
        public static Object WeaponAccessObject { get; set; } = new Object();
        public static Object GenerateRandomNumberAccess { get; set; } = new Object();
        public static Object ZombieHealthAccess { get; set; } = new Object();
        public static Object PlayerStunnedAccess { get; set; } = new Object();
        public static Object PlayerObjectAccess { get; set; } = new Object();
        public static Object ThreadListAccess { get; set; } = new Object();

        public static ManualResetEvent ReloadEvent { get; set; }
        public static ManualResetEvent ExitGame { get; set; }
        public static ManualResetEvent PlayerScoreStatusEvent { get; set; }
        public static ManualResetEvent KeyInfoWaitEvent { get; set; }
        public static ManualResetEvent PlayerHealthStatusEvent { get; set; }
        public static ManualResetEvent PlayerAmmoStatusEvent { get; set; }
        public static ManualResetEvent PlayerCurrentWeaponStatusEvent { get; set; }
        public static ManualResetEvent PauseGameEvent { get; set; }
        public static ManualResetEvent JumpWait { get; set; }

        public static PlayerModel PlayerModel { get; set; }
        public static HealthPackModel HealthPackModel { get; set; }
        public static AmmoPackModel AmmoPackModel { get; set; }
        public static AK47Model Ak47Model { get; set; }
        public static PistolBulletModel PistolBulletModel { get; set; }
        public static AK47BulletModel AK47BulletModel { get; set; }
        public static SniperRifleBulletModel SniperRifleBulletModel { get; set; }
        public static SniperRifleModel SniperRifleModel { get; set; }
        public static NormalZombieModel NormalZombieModel { get; set; }
        public static TankZombieModel TankZombieModel { get; set; }
        public static HunterZombieModel HunterZombieModel { get; set; }

        public static void ClearText(Position from, Position to)
        {
            for (int j = from.Y; j < to.Y; j++)
            {
                for (int i = from.X; i < to.X; i++)
                {
                    Console.SetCursorPosition(i, j);
                    Console.Write(' ');
                }
            }
        }

        public static void DrawText()
        {
            string line;
            try
            {
                int y = 3;
                StreamReader file = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\l4d.txt");

                while ((line = file.ReadLine()) != null)
                {
                    if (y >= 3 && y < 7)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (y == 7)
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(30, y++);
                    Console.WriteLine(line);
                }
                file.Close();
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (FileNotFoundException) { }
        }

        public static void InitializeGame()
        {
            Console.Clear();

            IDifficulty difficulty;

            if (GameData.Difficulty == "Hard")
            {
                difficulty = new HardDifficulty();
            }
            else if (GameData.Difficulty == "Medium")
            {
                difficulty = new MediumDifficulty();
            }
            else
            {
                GameData.Difficulty = "Easy";
                difficulty = new EasyDifficulty();
            }

            IInterfaceBuilder interfaceBuilder;

            if (GameData.InterfaceBuilder == "Mathemathics")
            {
                interfaceBuilder = new MathemathicsInterfaceBuilder();
            }
            else if (GameData.InterfaceBuilder == "Rainbow")
            {
                interfaceBuilder = new RainbowInterfaceBuilder();
            }
            else
            {
                interfaceBuilder = new DefaultInterfaceBuilder();
            }

            GameData.pause = new Pause();

            GameData.GameMechanics = new Mechanics(GameData.pause);

            PlayerStatusTracker playerStatusTracker = new PlayerStatusTracker(GameData.pause);

            GameData.SetGraphics(interfaceBuilder);
            Console.SetCursorPosition(105, 22);
            Console.Write("Difficulty: " + difficulty.ToString());
            Console.CursorVisible = false;

            GameData.gameMap = new char[50, 130];
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 130; j++)
                {
                    GameData.gameMap[i, j] = ' ';
                }
            }

            GameData.TankZombieModel = new TankZombieModel(new char[4] { '¥', '¥', '¥', '¥' }, ConsoleColor.Green, GameData.BackgroundColor);
            GameData.NormalZombieModel = new NormalZombieModel(new char[4] { '@', '@', '@', '@' }, ConsoleColor.Green, GameData.BackgroundColor);
            GameData.HunterZombieModel = new HunterZombieModel(new char[4] { 'x', 'x', 'x', 'x' }, ConsoleColor.Green, GameData.BackgroundColor);
            GameData.HealthPackModel = new HealthPackModel(new char[4] { '+', '+', '+', '+' }, ConsoleColor.Red, GameData.BackgroundColor);
            GameData.AmmoPackModel = new AmmoPackModel(new char[4] { '#', '#', '#', '#' }, ConsoleColor.Red, GameData.BackgroundColor);
            GameData.Ak47Model = new AK47Model(new char[4] { 'A', 'A', 'A', 'A' }, ConsoleColor.Red, GameData.BackgroundColor);
            GameData.SniperRifleModel = new SniperRifleModel(new char[4] { 'S', 'S', 'S', 'S' }, ConsoleColor.Red, GameData.BackgroundColor);
            GameData.PistolBulletModel = new PistolBulletModel(new char[4] { '.', '.', '.', '.' }, ConsoleColor.White, GameData.BackgroundColor);
            GameData.AK47BulletModel = new AK47BulletModel(new char[4] { '°', '°', '°', '°' }, ConsoleColor.White, GameData.BackgroundColor);
            GameData.SniperRifleBulletModel = new SniperRifleBulletModel(new char[4] { '│', '─', '│', '─' }, ConsoleColor.White, GameData.BackgroundColor);
            GameData.PlayerModel = new PlayerModel(new char[4] { '^', '>', 'v', '<' }, ConsoleColor.Green, GameData.BackgroundColor);

            GameData.ZombieAtPosition = new Dictionary<Position, Zombie>();
            GameData.DropAtPosition = new Dictionary<Position, Drop>();

            GameData.Player = Player.GetPlayer();

            Player.Name = GameData.PlayerName;
            Player.PlayerModel = new PlayerModel(new char[4] { '^', '>', 'v', '<' }, ConsoleColor.Green, GameData.BackgroundColor);
            Player.Score = 0;
            Player.Position = new Position(65, 25);
            Player.Health = 100;
            Player.Direction = Direction.DOWN;
            Player.Weapons = new List<Weapon>() { new Pistol() };
            Player.CurrentWeapon = Player.Weapons.ElementAt(0);
            GameData.DisplayModelAtPosition(Player.Position, Player.Position, Player.PlayerModel, Direction.DOWN, true);

            GameData.ReloadEvent = new ManualResetEvent(false);
            GameData.JumpWait = new ManualResetEvent(false);
            GameData.PauseGameEvent = new ManualResetEvent(false);
            GameData.PlayerHealthStatusEvent = new ManualResetEvent(false);
            GameData.KeyInfoWaitEvent = new ManualResetEvent(false);
            GameData.PlayerAmmoStatusEvent = new ManualResetEvent(false);
            GameData.PlayerCurrentWeaponStatusEvent = new ManualResetEvent(false);
            GameData.PlayerScoreStatusEvent = new ManualResetEvent(false);

            Player.IsReloaded = true;
            GameData.threadList = new List<Thread>();

            Thread playerScoreStatusThread = new Thread(() => playerStatusTracker.TrackPlayerScoreStatus());
            Thread playerCurrentWeaponStatusThread = new Thread(() => playerStatusTracker.TrackPlayerCurrentWeaponStatus());
            Thread playerAmmoStatusThread = new Thread(() => playerStatusTracker.TrackPlayerAmmoStatus());
            Thread playerHealthStatusThread = new Thread(() => playerStatusTracker.TrackPlayerHealthStatus());
            Thread reloadWeaponThread = new Thread(() => GameMechanics.ReloadWeapon());
            Thread zombieMaker = new Thread(() => new ZombieMaker(difficulty, GameData.pause).MakeZombies());
            Thread start = new Thread(() => GameData.GameMechanics.StartGame());

            GameData.AddThread(playerScoreStatusThread);
            GameData.AddThread(playerCurrentWeaponStatusThread);
            GameData.AddThread(playerAmmoStatusThread);
            GameData.AddThread(playerHealthStatusThread);
            GameData.AddThread(reloadWeaponThread);
            GameData.AddThread(zombieMaker);

            playerScoreStatusThread.Start();
            playerCurrentWeaponStatusThread.Start();
            playerAmmoStatusThread.Start();
            playerHealthStatusThread.Start();
            reloadWeaponThread.Start();
            zombieMaker.Start();
            start.Start();
        }

        public static void AddThread(Thread thread)
        {
            lock (GameData.ThreadListAccess)
            {
                GameData.threadList.Add(thread);
            }
        }

        public static void RemoveThread(Thread thread)
        {
            lock (GameData.ThreadListAccess)
            {
                GameData.threadList.Remove(thread);
            }
        }

        public static void RemoveAllThreads()
        {
            lock (GameData.ConsoleAccessObject)
            {
                foreach (Zombie zombie in ZombieAtPosition.Values)
                {
                    zombie.SetDrop(false);
                }
                foreach (Thread t in threadList)
                {
                    t.Interrupt();
                }
            }
        }

        public static void SetGraphics(IInterfaceBuilder interfaceBuilder)
        {
            int j = 0;

            InterfaceDirector director = new InterfaceDirector(interfaceBuilder);
            director.Construct();

            Product frame = interfaceBuilder.GetProduct();

            lock (GameData.ConsoleAccessObject)
            {
                Console.BackgroundColor = frame.BackgroundColor;
                GameData.BackgroundColor = frame.BackgroundColor;

                Console.Clear();
         
                for (int i = 0; i <= 82; i++)
                {
                    Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                    Console.SetCursorPosition(19 + i, 5);
                    Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                    Console.Write(frame.FrameCharacters[j].Character);
                    j++;
                }

                Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                Console.SetCursorPosition(102, 5);
                Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                Console.Write(frame.FrameCharacters[j].Character);
                j++;

                for (int i = 0; i <= 38; i++)
                {
                    Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                    Console.SetCursorPosition(102, 6 + i);
                    Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                    Console.Write(frame.FrameCharacters[j].Character);
                    j++;
                }

                Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                Console.SetCursorPosition(102, 45);
                Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                Console.Write(frame.FrameCharacters[j].Character);
                j++;

                for (int i = 0; i <= 82; i++)
                {
                    Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                    Console.SetCursorPosition(101 - i, 45);
                    Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                    Console.Write(frame.FrameCharacters[j].Character);
                    j++;
                }

                Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                Console.SetCursorPosition(18, 45);
                Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                Console.Write(frame.FrameCharacters[j].Character);
                j++;

                for (int i = 0; i <= 38; i++)
                {
                    Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                    Console.SetCursorPosition(18, 44 - i);
                    Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                    Console.Write(frame.FrameCharacters[j].Character);
                    j++;
                }
                Console.SetCursorPosition(18, 5);

                Console.ForegroundColor = frame.FrameCharacters[j].CharacterColor;
                Console.OutputEncoding = frame.FrameCharacters[j].Encoding;
                Console.Write(frame.FrameCharacters[j].Character);
            }

            Console.OutputEncoding = Encoding.UTF8;
        }

        public static bool ValidatePosition(Position position)
        {
            if (position.X >= 19 && position.X <= 101)
            {
                if (position.Y >= 6 && position.Y <= 44)
                {
                    return true;
                }
            }
            return false;
        }

        public static Collision CheckForCollision(Position position)
        {
            if (ValidatePosition(position) == true)
            {
                lock (ConsoleAccessObject)
                {
                    if (gameMap[position.Y, position.X] != ' ')
                    {
                        if (NormalZombieModel.Equals(gameMap[position.Y, position.X]) || HunterZombieModel.Equals(gameMap[position.Y, position.X])
                            || TankZombieModel.Equals(gameMap[position.Y, position.X]))
                        {
                            return Collision.ZOMBIE;
                        }
                        else if (GameData.PlayerModel.Equals(gameMap[position.Y, position.X]))
                        {
                            return Collision.PLAYER;
                        }
                        else if (Player.CurrentWeapon.BulletModel.Equals(gameMap[position.Y, position.X]))
                        {
                            return Collision.BULLET;
                        }
                        else
                        {
                            return Collision.DROP;
                        }
                    }
                    else
                    {
                        return Collision.NONE;
                    }
                }
            }
            else
            {
                return Collision.WALL;
            }
        }

        public static void AddZombieAtPosition(Position position, Zombie zombie)
        {
            lock (ConsoleAccessObject)
            {
                lock (ZombieAtPositionDictionaryAccess)
                {
                    ZombieAtPosition.Add(position, zombie);
                }
                DisplayModelAtPosition(position, position, zombie.ZombieModel, Direction.DOWN, true);
            }
        }

        public static void RemoveZombieAtPosition(Position position)
        {
            lock (ConsoleAccessObject)
            {
                lock (ZombieAtPositionDictionaryAccess)
                {
                    Zombie zombie = ZombieAtPosition[position];

                    lock (JumpThreadAccess)
                    {
                        zombie.Kill();
                    }
                }
            }
        }

        public static void AddDropAtPosition(Position position, Drop drop)
        {
            lock (ConsoleAccessObject)
            {
                lock (DropAtPositionDictionaryAccess)
                {
                    DropAtPosition.Add(position, drop);
                }
                DisplayModelAtPosition(position, position, drop.GetModel(), Direction.DOWN, false);
            }
        }

        public static void RemoveDropAtPosition(Position position)
        {
            lock (DropAtPositionDictionaryAccess)
            {
                Position _position = DropAtPosition.Keys.Where(p => p.Equals(position)).FirstOrDefault();

                if (_position != null)
                {
                    DropAtPosition[_position].GetTrackingThread().Interrupt();
                    DropAtPosition.Remove(_position);
                    ClearPosition(_position);
                }
            }
        }

        public static int GenerateRandomNumber(int lower = 0, int upper = 0)
        {
            lock (GenerateRandomNumberAccess)
            {
                return new Random().Next(lower, upper);
            }
        }

        public static void DisplayModelAtPosition(Position nextPosition, Position position, Model model, Direction dir, bool clearPrevious = true)
        {
            lock (ConsoleAccessObject)
            {
                Console.ForegroundColor = model.ModelColor;
                Console.BackgroundColor = model.BackgroundColor;

                if (!position.Equals(nextPosition))
                {
                    if (clearPrevious)
                        ClearPosition(position);

                    position.X = nextPosition.X;
                    position.Y = nextPosition.Y;

                    Console.SetCursorPosition(position.X, position.Y);
                    gameMap[position.Y, position.X] = model.DisplayModel((int)dir);
                    Console.Write(model.DisplayModel((int)dir));
                }
                else
                {
                    gameMap[position.Y, position.X] = model.DisplayModel((int)dir);
                    Console.SetCursorPosition(position.X, position.Y);
                    Console.Write(model.DisplayModel((int)dir));
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = GameData.BackgroundColor;
            }
        }

        static public void ClearPosition(Position position)
        {
            lock (ConsoleAccessObject)
            {
                Console.SetCursorPosition(position.X, position.Y);
                gameMap[position.Y, position.X] = ' ';
                Console.Write(" ");
            }
        }
    }
}
