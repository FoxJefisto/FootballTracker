using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class RowPlayerStatistics
    {
        public RowPlayerStatistics(FootballClub club, Competitions competition,
            int matches, int goals, int pengoals, int assists, int minutes,
            int inStartMatches, int inSubsMatches, int yellowCards,
            int yellowRedCards, int redCards)
        {
            Club = club;
            Competition = competition;
            Matches = matches;
            Goals = goals;
            PenGoals = pengoals;
            Assists = assists;
            Minutes = minutes;
            InStartMatches = inStartMatches;
            InSubsMatches = inSubsMatches;
            YellowCards = yellowCards;
            YellowRedCards = yellowRedCards;
            RedCards = redCards;
        }
        public FootballClub Club { get; private set; }
        public Competitions Competition { get; private set; }
        public int Matches { get; private set; }
        public int Goals { get; private set; }
        public int PenGoals { get; private set; }
        public int Assists { get; private set; }
        public int Minutes { get; private set; }
        public int InStartMatches { get; private set; }
        public int InSubsMatches { get; private set; }
        public int YellowCards { get; private set; }
        public int YellowRedCards { get; private set; }
        public int RedCards { get; private set; }
    }
}
