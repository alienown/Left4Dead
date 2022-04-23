using System;
using System.Threading;

namespace Left4Dead
{
    public class Program
    {
        static void Main(string[] args)
        {
            GameData.ExitGame = new ManualResetEvent(false);
            Console.SetWindowSize(130, 50);
            Console.CursorVisible = false;

            while (true)
            {
                Menus.MainMenu();
            }
        }
    }
}