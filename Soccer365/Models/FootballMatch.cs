using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballMatch
    {
        public FootballMatch() { }
        public FootballMatch(string matchId, FootballClub clubHome, FootballClub clubAway, string matchStatus, int? clubHomeGoals, int? clubAwayGoals)
        {
            MatchId = matchId;
            ClubHome = clubHome;
            ClubAway = clubAway;
            MatchStatus = matchStatus;
            ClubHomeGoals = clubHomeGoals;
            ClubAwayGoals = clubAwayGoals;
            Winner = getWinner();
            MatchScore = getMatchScore();
        }

        private FootballClub getWinner()
        {
            FootballClub winner;
            if (ClubHomeGoals.HasValue && ClubAwayGoals.HasValue)
            {
                winner = ClubHomeGoals > ClubAwayGoals ? ClubHome : ClubAway;
            }
            else
                winner = null;
            return winner;
        }

        private string getMatchScore()
        {
            string score;
            if (ClubHomeGoals.HasValue && ClubAwayGoals.HasValue)
                score = ClubHomeGoals + " : " + ClubAwayGoals;
            else
                score = "- : -";
            return score;
        }
        public string MatchId { get; private set; }
        public FootballClub ClubHome { get; private set; }
        public FootballClub ClubAway { get; private set; }
        public string MatchStatus { get; private set; }
        public FootballClub Winner { get; private set; }
        public int? ClubHomeGoals { get; private set; }
        public int? ClubAwayGoals { get; private set; }
        public string MatchScore { get; private set; }
    }
}
