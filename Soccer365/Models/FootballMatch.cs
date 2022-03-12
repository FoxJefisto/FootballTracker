using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballMatch
    {
        public FootballMatch() { }
        public FootballMatch(string matchId, Pair<FootballClub> clubs, string matchStatus, Pair<int?> goals)
        {
            MatchId = matchId;
            Clubs = clubs;
            MatchStatus = matchStatus;
            Goals = goals;
            Winner = getWinner();
            MatchScore = getMatchScore();
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
        public string MatchId { get; private set; }
        public Pair<FootballClub> Clubs { get; private set; }
        public string MatchStatus { get; private set; }
        public FootballClub Winner { get; private set; }
        public Pair<int?> Goals { get; private set; }
        public string MatchScore { get; private set; }
    }
}
