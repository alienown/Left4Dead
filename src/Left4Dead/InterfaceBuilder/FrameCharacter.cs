using System;
using System.Text;

namespace Left4Dead
{
    public class FrameCharacter
    {
        public ConsoleColor CharacterColor { get; private set; }
        public char Character { get; private set; }
        public Encoding Encoding { get; private set; }

        public FrameCharacter(ConsoleColor color, char character, Encoding encoding)
        {
            Character = character;
            CharacterColor = color;
            Encoding = encoding;
        }
    }
}
