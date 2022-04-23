using System;
using System.Collections.Generic;

namespace Left4Dead
{
    public class Product
    {
        public ConsoleColor BackgroundColor { get; set; }
        public List<FrameCharacter> FrameCharacters { get; set; }

        public Product()
        {
            FrameCharacters = new List<FrameCharacter>();
        }
    }
}
