using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Left4Dead
{
    public static class Menus
    {
        public static void MainMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "START", "SCOREBOARD", "HOW TO PLAY", "OPTIONS", "CREDITS", "QUIT" };
            int ptr = 0;
            GameData.Option = options[ptr];
            int y;
            Console.ResetColor();
            Console.Clear();
            GameData.DrawText();
            do
            {
                y = 15;

                Console.SetCursorPosition(60, y);
                foreach (var option in options)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (options[ptr] == option)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;


                    }
                    Console.SetCursorPosition(60, y++);
                    Console.Write(option);
                }

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (ptr - 1 == -1)
                        {
                            ptr = 5;
                            GameData.Option = options[ptr];
                            Console.ResetColor();
                        }
                        else
                        {
                            GameData.Option = options[ptr--];
                        }
                        Console.ResetColor();
                        break;
                    case ConsoleKey.DownArrow:
                        if (ptr + 1 == 6)
                        {
                            ptr = 0;
                            GameData.Option = options[ptr];
                        }
                        else
                        {
                            GameData.Option = options[ptr++];
                        }
                        Console.ResetColor();
                        break;
                }
            } while (keyInfo.Key != ConsoleKey.Enter);
            GameData.Option = options[ptr];
            Console.Clear();


            if (GameData.Option == "SCOREBOARD")
            {
                Menus.ScoreboardMenu();
            }
            if (GameData.Option == "QUIT")
            {
                Environment.Exit(0);
            }
            if (GameData.Option == "CREDITS")
            {
                Menus.CreditsMenu();
            }
            if (GameData.Option == "OPTIONS")
            {
                Menus.OptionsMenu();
            }
            if (GameData.Option == "HOW TO PLAY")
            {
                Menus.HowToPlayMenu();
            }
            if (GameData.Option == "START")
            {
                Menus.NameInputMenu();
                GameData.InitializeGame();
                GameData.ExitGame.Reset();
                GameData.ExitGame.WaitOne();
            }
        }

        public static void CreditsMenu()
        {

            Console.Clear();
            GameData.DrawText();
            ConsoleKeyInfo keyInfo;
            Console.SetCursorPosition(35, 15);
            Console.WriteLine("GRA ZOSTALA STWORZONA PRZEZ :");
            Console.SetCursorPosition(35, 16);
            Console.WriteLine();
            Console.SetCursorPosition(35, 17);
            Console.WriteLine("KAMIL KAPLINSKI ORAZ LUKASZ HRYNIEWICKI");
            Console.SetCursorPosition(35, 18);
            Console.WriteLine();
            Console.SetCursorPosition(35, 19);
            Console.WriteLine("SPECJALNE PODZIEKOWANIA DLA ZSAKUL REMODAG ZA OKAZJE DO STWORZENIA TEJ PIEKNEJ GRY");
            Console.SetCursorPosition(35, 20);
            Console.WriteLine();
            Console.SetCursorPosition(53, 35);
            Console.WriteLine("ESC - BACK TO MAIN MENU");
            do
            {
                keyInfo = Console.ReadKey(true);

            } while (keyInfo.Key != ConsoleKey.Escape);
        }

        public static void DifficultyMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "EASY", "MEDIUM", "HARD" };
            int ptr = 0;
            GameData.Option = options[ptr];
            int y;
            bool done = false;
            Console.ResetColor();
            Console.Clear();
            Console.SetCursorPosition(55, 35);
            Console.WriteLine("ESC - BACK TO OPTIONS");
            GameData.DrawText();
            do
            {
                do
                {
                    y = 15;
                    Console.SetCursorPosition(60, y);
                    foreach (var option in options)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (options[ptr] == option)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;


                        }
                        Console.SetCursorPosition(60, y++);
                        Console.Write(option);
                    }

                    keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (ptr - 1 == -1)
                            {
                                ptr = 2;
                                GameData.Option = options[ptr];
                                Console.ResetColor();
                            }
                            else
                            {
                                GameData.Option = options[ptr--];
                            }
                            Console.ResetColor();
                            break;
                        case ConsoleKey.DownArrow:
                            if (ptr + 1 == 3)
                            {
                                ptr = 0;
                                GameData.Option = options[ptr];
                            }
                            else
                            {
                                GameData.Option = options[ptr++];
                            }
                            Console.ResetColor();
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.ResetColor();
                    break;
                }

                GameData.Option = options[ptr];


                switch (GameData.Option)
                {
                    case "EASY":
                        GameData.Difficulty = "Easy";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Difficulty set to easy");
                        break;
                    case "MEDIUM":
                        GameData.Difficulty = "Medium";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Difficulty set to medium");
                        break;
                    case "HARD":
                        GameData.Difficulty = "Hard";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Difficulty set to hard");
                        break;
                }

            } while (!done);
        }

        public static void GraphicsMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "DEFAULT", "MATHEMATHICS", "RAINBOW" };
            int ptr = 0;
            GameData.Option = options[ptr];
            int y;
            bool done = false;
            Console.ResetColor();
            Console.Clear();
            Console.SetCursorPosition(55, 35);
            Console.WriteLine("ESC - BACK TO OPTIONS");
            GameData.DrawText();
            do
            {
                do
                {
                    y = 15;

                    Console.SetCursorPosition(60, y);
                    foreach (var option in options)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (options[ptr] == option)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;


                        }
                        Console.SetCursorPosition(60, y++);
                        Console.Write(option);
                    }

                    keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (ptr - 1 == -1)
                            {
                                ptr = 2;
                                GameData.Option = options[ptr];
                                Console.ResetColor();
                            }
                            else
                            {
                                GameData.Option = options[ptr--];
                            }
                            Console.ResetColor();
                            break;
                        case ConsoleKey.DownArrow:
                            if (ptr + 1 == 3)
                            {
                                ptr = 0;
                                GameData.Option = options[ptr];
                            }
                            else
                            {
                                GameData.Option = options[ptr++];
                            }
                            Console.ResetColor();
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.ResetColor();
                    break;
                }

                GameData.Option = options[ptr];

                switch (GameData.Option)
                {
                    case "DEFAULT":
                        GameData.InterfaceBuilder = "Default";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Frame style set to default");
                        break;
                    case "MATHEMATHICS":
                        GameData.InterfaceBuilder = "Mathemathics";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Frame style set to mathemathics");
                        break;
                    case "RAINBOW":
                        GameData.InterfaceBuilder = "Rainbow";
                        Console.ResetColor();
                        Console.SetCursorPosition(60, 25);
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Frame style set to rainbow");
                        break;
                }

            } while (!done);
        }

        public static void OptionsMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "DIFFICULTY", "GRAPHICS" };
            int ptr = 0;
            GameData.Option = options[ptr];
            int y;
            bool done = false;
            do
            {
                Console.Clear();
                GameData.DrawText();
                Console.ResetColor();
                Console.SetCursorPosition(53, 35);
                Console.WriteLine("ESC - BACK TO MAIN MENU");
                do
                {
                    y = 15;
                    Console.SetCursorPosition(60, y);
                    foreach (var option in options)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (options[ptr] == option)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;


                        }
                        Console.SetCursorPosition(60, y++);
                        Console.Write(option);
                    }

                    keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (ptr - 1 == -1)
                            {
                                ptr = 1;
                                GameData.Option = options[ptr];
                                Console.ResetColor();
                            }
                            else
                            {
                                GameData.Option = options[ptr--];
                            }
                            Console.ResetColor();
                            break;
                        case ConsoleKey.DownArrow:
                            if (ptr + 1 == 2)
                            {
                                ptr = 0;
                                GameData.Option = options[ptr];
                            }
                            else
                            {
                                GameData.Option = options[ptr++];
                            }
                            Console.ResetColor();
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.ResetColor();
                    break;
                }

                GameData.Option = options[ptr];
                Console.Clear();

                switch (GameData.Option)
                {
                    case "DIFFICULTY":
                        Menus.DifficultyMenu();
                        break;
                    case "GRAPHICS":
                        Menus.GraphicsMenu();
                        break;
                }

            } while (!done);
        }

        public static void HowToPlayMenu()
        {
            Console.Clear();
            GameData.DrawText();
            ConsoleKeyInfo keyInfo;
            Console.SetCursorPosition(60, 15);
            Console.WriteLine("HOW TO PLAY");
            Console.SetCursorPosition(15, 17);
            Console.WriteLine("MOVEMENT:");
            Console.SetCursorPosition(15, 19);
            Console.WriteLine("LEFT - ←");
            Console.SetCursorPosition(15, 20);
            Console.WriteLine("RIGHT - →");
            Console.SetCursorPosition(15, 21);
            Console.WriteLine("UP - ↑");
            Console.SetCursorPosition(15, 22);
            Console.WriteLine("DOWN - ↓");
            Console.SetCursorPosition(15, 23);
            Console.WriteLine("SHOOT - SPACEBAR");
            Console.SetCursorPosition(15, 24);
            Console.WriteLine("PISTOL - 1");
            Console.SetCursorPosition(15, 25);
            Console.WriteLine("AK47 - 2");
            Console.SetCursorPosition(15, 26);
            Console.WriteLine("SNIPER RIFLE - 3");
            Console.SetCursorPosition(53, 35);
            Console.WriteLine("ESC - BACK TO MAIN MENU");

            Console.SetCursorPosition(35, 17);
            Console.WriteLine("ZOMBIES:");
            Console.SetCursorPosition(35, 19);
            Console.WriteLine("@ - Normal zombie, slow movement, little damage,");
            Console.SetCursorPosition(40, 20);
            Console.WriteLine("litle health, low drop rate");
            Console.SetCursorPosition(35, 22);
            Console.WriteLine("Y - Tank, fast movement, huge damage, huge health,");
            Console.SetCursorPosition(40, 23);
            Console.WriteLine("can stun and push you away, high drop rate");
            Console.SetCursorPosition(35, 25);
            Console.WriteLine("x - Hunter, very fast movement, medium damage,");
            Console.SetCursorPosition(40, 26);
            Console.WriteLine("medium health, can jump to you, medium drop rate");


            Console.SetCursorPosition(90, 17);
            Console.Write("ITEMS:");
            Console.SetCursorPosition(90, 19);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("#");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Ammo pack");
            Console.SetCursorPosition(90, 20);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("+");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Health pack");
            Console.SetCursorPosition(90, 21);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("A");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - AK47");
            Console.SetCursorPosition(90, 22);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("S");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Sniper Rifle");
            Console.SetCursorPosition(90, 23);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("#");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Yellow foreground on drop");
            Console.SetCursorPosition(94, 24);
            Console.Write("stands for bonus ammo");
            Console.SetCursorPosition(90, 25);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Green;
            Console.Write("+");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Green background on drop");
            Console.SetCursorPosition(94, 26);
            Console.Write("stands for bonus health");

            do
            {
                keyInfo = Console.ReadKey(true);

            } while (keyInfo.Key != ConsoleKey.Escape);
        }

        public static void NameInputMenu()
        {
            string name = " ";
            Console.Clear();
            Console.SetCursorPosition(60, 20);
            Console.Write("Please enter Your name");
            Console.SetCursorPosition(60, 24);
            Console.Write("*Your name cannot contain white spaces");
            Console.SetCursorPosition(60, 22);
            Console.Write("Name: ");
            do
            {
                Console.SetCursorPosition(66, 22);
                foreach (char c in name)
                {
                    Console.Write(" ");
                }
                Console.SetCursorPosition(66, 22);
                name = Console.ReadLine();
                GameData.PlayerName = name;
            } while (String.IsNullOrWhiteSpace(name) || name.Contains(" "));
        }

        public static void ScoreboardMenu()
        {
            Console.Clear();
            ConsoleKeyInfo keyInfo;
            GameData.DrawText();
            string line;
            string[] data = new string[3];
            int pageNumber = 1;
            Score score;

            Console.SetCursorPosition(55, 15);
            Console.Write("Name: ");
            Console.Write("Score: ");
            Console.Write("Difficulty: ");

            Console.SetCursorPosition(47, 40);
            Console.Write("Previous page: <-      ");
            Console.Write("Next page: ->");
            Console.SetCursorPosition(62, 42);
            Console.Write("Page " + pageNumber);
            Console.SetCursorPosition(53, 45);
            Console.Write("ESC - BACK TO MAIN MENU");

            int x = 55, y = 17;

            ScoreAggregate scoreAggregate = new ScoreAggregate();

            try
            {
                StreamReader file = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ranking.txt");

                while ((line = file.ReadLine()) != null)
                {
                    data = line.Split(' ');
                    score = new Score(data[0], data[1], data[2]);
                    scoreAggregate.AddScore(score);
                }
                file.Close();

                IPageIterator scoreIterator = scoreAggregate.CreateIterator();

                Console.SetCursorPosition(x, y);
                foreach (Score s in ((IEnumerable<Score>)scoreIterator.GetPage()))
                {
                    Console.Write(s);
                    y += 2;
                    Console.SetCursorPosition(x, y);
                }

                do
                {
                    keyInfo = Console.ReadKey(true);
                    y = 17;
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.RightArrow:
                            if (scoreIterator.HasNextPage())
                            {
                                pageNumber += 1;
                                GameData.ClearText(new Position(0, 17), new Position(130, 39));
                                scoreIterator.NextPage();
                                Console.SetCursorPosition(x, y);
                                foreach (Score s in ((IEnumerable<Score>)scoreIterator.GetPage()))
                                {
                                    Console.Write(s);
                                    y += 2;
                                    Console.SetCursorPosition(x, y);
                                }
                                Console.SetCursorPosition(67, 42);
                                Console.Write(pageNumber);
                            }
                            break;
                        case ConsoleKey.LeftArrow:
                            if (scoreIterator.HasPreviousPage())
                            {
                                pageNumber -= 1;
                                GameData.ClearText(new Position(0, 17), new Position(130, 39));
                                scoreIterator.PreviousPage();
                                Console.SetCursorPosition(x, y);
                                foreach (Score s in ((IEnumerable<Score>)scoreIterator.GetPage()))
                                {
                                    Console.Write(s);
                                    y += 2;
                                    Console.SetCursorPosition(x, y);
                                }
                                Console.SetCursorPosition(67, 42);
                                Console.Write("         ");
                                Console.SetCursorPosition(67, 42);
                                Console.Write(pageNumber);
                            }
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Escape);

            }
            catch (FileNotFoundException) { }
        }

        public static void GameOverMenu()
        {
            Console.Clear();
            string[] options = new string[] { "TRY AGAIN", "EXIT TO MAIN MENU" };
            int ptr = 0;
            GameData.Option = options[ptr];
            int x;
            string chain = GameData.Player.Name + ", Your score is: " + GameData.Player.Score;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(60, 18);
            Console.Write("GAME OVER");

            Console.SetCursorPosition(64 - chain.Length / 2, 20);
            Console.Write(chain);

            do
            {
                x = 51;
                Console.SetCursorPosition(x, 22);
                foreach (var option in options)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (options[ptr] == option)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.SetCursorPosition(x, 24);
                    Console.Write(option);
                    x += options[0].Length + 1;
                }

                GameData.KeyInfoWaitEvent.Reset();
                GameData.KeyInfoWaitEvent.WaitOne();

                switch (GameData.KeyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (ptr - 1 == -1)
                        {
                            ptr = 1;
                            GameData.Option = options[ptr];
                            Console.ResetColor();
                        }
                        else
                        {
                            GameData.Option = options[ptr--];
                        }
                        Console.ResetColor();
                        break;
                    case ConsoleKey.RightArrow:
                        if (ptr + 1 == 2)
                        {
                            ptr = 0;
                            GameData.Option = options[ptr];
                        }
                        else
                        {
                            GameData.Option = options[ptr++];
                        }
                        Console.ResetColor();
                        break;
                }
            } while (GameData.KeyInfo.Key != ConsoleKey.Enter);
            GameData.Option = options[ptr];
        }

        public static void PauseMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "RESUME", "EXIT TO MAIN MENU" };
            int ptr = 0;
            GameData.Option = options[ptr];
            int x;

            Console.BackgroundColor = GameData.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(110, 26);
            Console.Write("GAME IS PAUSED");

            do
            {
                x = 105;
                Console.SetCursorPosition(x, 28);
                foreach (var option in options)
                {
                    Console.BackgroundColor = GameData.BackgroundColor;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (options[ptr] == option)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.SetCursorPosition(x, 28);
                    Console.Write(option);
                    x += options[0].Length + 1;
                }

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (ptr - 1 == -1)
                        {
                            ptr = 1;
                            GameData.Option = options[ptr];
                            Console.ResetColor();
                        }
                        else
                        {
                            GameData.Option = options[ptr--];
                        }
                        Console.ResetColor();
                        break;
                    case ConsoleKey.RightArrow:
                        if (ptr + 1 == 2)
                        {
                            ptr = 0;
                            GameData.Option = options[ptr];
                        }
                        else
                        {
                            GameData.Option = options[ptr++];
                        }
                        Console.ResetColor();
                        break;
                }
            } while (keyInfo.Key != ConsoleKey.Enter);
            GameData.Option = options[ptr];

            Console.BackgroundColor = GameData.BackgroundColor;
        }
    }
}
