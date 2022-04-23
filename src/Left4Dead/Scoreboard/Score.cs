namespace Left4Dead
{
    public class Score
    {
        public string PlayerName { get; set; }
        public string PlayerScore { get; set; }
        public string GameDifficulty { get; set; }

        public Score(string playerName, string playerScore, string gameDifficulty)
        {
            this.PlayerName = playerName;
            this.PlayerScore = playerScore;
            this.GameDifficulty = gameDifficulty;
        }

        public override string ToString()
        {
            return PlayerName + " " + PlayerScore + " " + GameDifficulty;
        }
    }
}
