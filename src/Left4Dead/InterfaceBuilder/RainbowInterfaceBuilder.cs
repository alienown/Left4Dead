using System;
using System.Text;

namespace Left4Dead
{
    public class RainbowInterfaceBuilder : IInterfaceBuilder
    {
        private Product gameInterface;
        private ConsoleColor[] rainbowColors;
        private int nextColor;

        public RainbowInterfaceBuilder()
        {
            gameInterface = new Product();

            rainbowColors = new ConsoleColor[]
            {
                ConsoleColor.Red,
                ConsoleColor.DarkYellow,
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Cyan,
                ConsoleColor.Magenta
            };

            nextColor = 0;
        }

        public Product GetProduct()
        {
            return gameInterface;
        }

        public void SetBackgroundColor()
        {
            gameInterface.BackgroundColor = ConsoleColor.Blue;
        }

        public void SetLeftSide()
        {
            for (int i = 0; i <= 38; i++)
            {
                gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '║', Encoding.UTF8));
                nextColor++;
            }
        }

        public void SetLowerLeftCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╚', Encoding.UTF8));
            nextColor++;
        }

        public void SetLowerRightCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╝', Encoding.UTF8));
            nextColor++;
        }

        public void SetLowerSide()
        {
            for (int i = 0; i <= 82; i++)
            {
                gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '═', Encoding.UTF8));
                nextColor++;
            }
        }

        public void SetRightSide()
        {
            for (int i = 0; i <= 38; i++)
            {
                gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '║', Encoding.UTF8));
                nextColor++;
            }
        }

        public void SetUpperLeftCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╔', Encoding.UTF8));
            nextColor++;
        }

        public void SetUpperRightCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╗', Encoding.UTF8));
            nextColor++;
        }

        public void SetUpperSide()
        {
            for (int i = 0; i <= 82; i++)
            {
                gameInterface.FrameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '═', Encoding.UTF8));
                nextColor++;
            }
        }
    }
}
