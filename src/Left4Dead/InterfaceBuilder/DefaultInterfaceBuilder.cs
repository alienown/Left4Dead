using System;
using System.Text;

namespace Left4Dead
{
    public class DefaultInterfaceBuilder : IInterfaceBuilder
    {
        private Product gameInterface;

        public DefaultInterfaceBuilder()
        {
            gameInterface = new Product();
        }

        public Product GetProduct()
        {
            return gameInterface;
        }

        public void SetBackgroundColor()
        {
            gameInterface.BackgroundColor = ConsoleColor.Black;
        }

        public void SetLeftSide()
        {
            for (int i = 0; i <= 38; i++)
                gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '|', Encoding.ASCII));
        }

        public void SetLowerLeftCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '<', Encoding.ASCII));
        }

        public void SetLowerRightCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '.', Encoding.ASCII));
        }

        public void SetLowerSide()
        {
            for (int i = 0; i <= 82; i++)
                gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '=', Encoding.ASCII));
        }

        public void SetRightSide()
        {
            for (int i = 0; i <= 38; i++)
                gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '|', Encoding.ASCII));
        }

        public void SetUpperLeftCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '.', Encoding.ASCII));
        }

        public void SetUpperRightCorner()
        {
            gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '.', Encoding.ASCII));
        }

        public void SetUpperSide()
        {
            for (int i = 0; i <= 82; i++)
                gameInterface.FrameCharacters.Add(new FrameCharacter(ConsoleColor.White, '=', Encoding.ASCII));
        }
    }
}
