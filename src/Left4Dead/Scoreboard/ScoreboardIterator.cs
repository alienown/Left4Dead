using System.Collections.Generic;

namespace Left4Dead
{
    public class ScoreIterator : IPageIterator
    {
        private ScoreAggregate aggregate;
        private int current;

        public ScoreIterator(ScoreAggregate aggregate)
        {
            this.aggregate = aggregate;
            current = 0;
        }

        public object GetPage()
        {
            return aggregate[current];
        }

        public bool HasNextPage()
        {
            if (((ICollection<Score>)aggregate[current + 1]).Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasPreviousPage()
        {
            if (current == 0)
            {
                return false;
            }

            if (((ICollection<Score>)aggregate[current - 1]).Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void NextPage()
        {
            current += 1;
        }

        public void PreviousPage()
        {
            current -= 1;
        }
    }
}
