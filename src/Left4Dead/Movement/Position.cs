namespace Left4Dead
{
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position(Position position)
        {
            X = position.X;
            Y = position.Y;
        }

        public bool Equals(Position a)
        {
            if (a.X == X && a.Y == Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
