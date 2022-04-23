using System;
using System.Collections.Generic;

namespace Left4Dead
{
    public class ScoreAggregate : IAggregate
    {
        private List<Score> scoreList;

        public ScoreAggregate()
        {
            scoreList = new List<Score>();
        }

        public IPageIterator CreateIterator()
        {
            return new ScoreIterator(this);
        }

        public void AddScore(Score score)
        {
            scoreList.Add(score);
        }

        public object this[int index]
        {
            get
            {
                try
                {
                    return scoreList.GetRange(index * 10, 10);
                }
                catch (ArgumentException)
                {
                    if (scoreList.Count - index * 10 < 0)
                    {
                        return new List<Score>();
                    }

                    return scoreList.GetRange(index * 10, scoreList.Count - index * 10);
                }
            }
        }
    }
}
