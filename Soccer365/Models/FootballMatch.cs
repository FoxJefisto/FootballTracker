namespace Soccer365.Models
{
    public class FootballMatch
    {
        public FootballMatch() { }
        public FootballMatch(string matchId, Competitions competition, string stage,Pair<FootballClub> clubs, string matchStatus, Pair<int?> goals)
        {
            Id = matchId;
            Competition = competition;
            Stage = stage;
            Clubs = clubs;
            Status = matchStatus;
            Goals = goals;
            Winner = getWinner();
            Score = getMatchScore();
        }

        private FootballClub getWinner()
        {
            FootballClub winner;
            if (Goals.HomeTeam.HasValue && Goals.AwayTeam.HasValue)
            {
                winner = Goals.HomeTeam > Goals.AwayTeam ? Clubs.HomeTeam : Clubs.AwayTeam;
            }
            else
                winner = null;
            return winner;
        }

        private string getMatchScore()
        {
            string score;
            if (Goals.HomeTeam.HasValue && Goals.AwayTeam.HasValue)
                score = Goals.HomeTeam + " : " + Goals.AwayTeam;
            else
                score = "- : -";
            return score;
        }
        public Competitions Competition { get; private set; }
        public string Stage { get; private set; }
        public string Id { get; private set; }
        public Pair<FootballClub> Clubs { get; private set; }
        public string Status { get; private set; }
        public FootballClub Winner { get; private set; }
        public Pair<int?> Goals { get; private set; }
        public string Score { get; private set; }
    }
}
