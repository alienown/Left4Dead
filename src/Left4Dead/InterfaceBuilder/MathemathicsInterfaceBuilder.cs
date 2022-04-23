using System;
using System.Text;

namespace Left4Dead
{
    public class MathemathicsInterfaceBuilder : IInterfaceBuilder
    {
        private Product gameInterface;

        public MathemathicsInterfaceBuilder()
        {
            gameInterface = new Product();
        }

        public Product GetProduct()
        {
            return gameInterface;
        }

        public void SetBackgroundColor()
        {
            gameInterface.BackgroundColor = ConsoleColor.DarkGray;
        }

        public void SetLeftSide()
        {
            int j = 0;
            for (int i = 0; i <= 38; i++)
            {
                if (j == 0)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F6', Encoding.UTF8));
                }
                else if (j == 1)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FB', Encoding.UTF8));
                }
                else if (j == 2)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25EF', Encoding.UTF8));
                }
                else if (j == 3)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25AD', Encoding.UTF8));
                }
                else if (j == 4)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25B3', Encoding.UTF8));
                }
                else if (j == 5)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25D8', Encoding.UTF8));
                    j = 0;
                }
                j++;
            }
        }

        public void SetLowerLeftCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FA', Encoding.UTF8));
        }

        public void SetLowerRightCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FF', Encoding.UTF8));
        }

        public void SetLowerSide()
        {
            int j = 49;
            for (int i = 0; i <= 82; i++)
            {
                if (j == 49)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 50)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 51)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 52)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 53)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 54)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                    j = 49;
                }
                j++;
            }
        }

        public void SetRightSide()
        {
            int j = 0;
            for (int i = 0; i <= 38; i++)
            {
                if (j == 0)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F6', Encoding.UTF8));
                }
                else if (j == 1)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FB', Encoding.UTF8));
                }
                else if (j == 2)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25EF', Encoding.UTF8));
                }
                else if (j == 3)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25AD', Encoding.UTF8));
                }
                else if (j == 4)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25B3', Encoding.UTF8));
                }
                else if (j == 5)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25D8', Encoding.UTF8));
                    j = 0;
                }
                j++;
            }
        }

        public void SetUpperLeftCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F8', Encoding.UTF8));
        }

        public void SetUpperRightCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F9', Encoding.UTF8));
        }

        public void SetUpperSide()
        {
            int j = 49;
            for (int i = 0; i <= 82; i++)
            {
                if (j == 49)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 50)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 51)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 52)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 53)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 54)
                {
                    gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                    j = 49;
                }
                j++;
            }

        }
    }
}
