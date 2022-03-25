namespace Soccer365.Models
{
    public class RowSquadPlayerStatistics
    {
        public RowSquadPlayerStatistics(Person player, int? number, string position, int goals, int assists, 
            int matches, int minutes, int goalPlusPass, int penGoals, int doubleGoals, int hatTricks, int autoGoals,
            int yellowCards, int yellowRedCards, int redCards, int fairPlayScore)
        {
            Player = player;
            Number = number;
            Position = position;
            Goals = goals;
            Assists = assists;
            Matches = matches;
            Minutes = minutes;
            GoalPlusPass = goalPlusPass;
            PenGoals = penGoals;
            DoubleGoals = doubleGoals;
            HatTricks = hatTricks;
            AutoGoals = autoGoals;
            YellowCards = yellowCards;
            YellowRedCards = yellowRedCards;
            RedCards = redCards;
            FairPlayScore = fairPlayScore;
        }

        public Person Player { get; private set; }
        public int? Number { get; private set; }
        public string Position { get; private set; }
        public int Goals { get; private set; } = 0;
        public int Assists { get; private set; } = 0;
        public int Matches { get; private set; } = 0;
        public int Minutes { get; private set; } = 0;
        public int GoalPlusPass { get; private set; } = 0;
        public int PenGoals { get; private set; } = 0;
        public int DoubleGoals { get; private set; } = 0;
        public int HatTricks { get; private set; } = 0;
        public int AutoGoals { get; private set; } = 0;
        public int YellowCards { get; private set; } = 0;
        public int YellowRedCards { get; private set; } = 0;
        public int RedCards { get; private set; } = 0;
        public int FairPlayScore { get; private set; } = 0;
    }
}
