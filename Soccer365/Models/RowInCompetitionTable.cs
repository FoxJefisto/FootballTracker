using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class RowInCompetitionTable
    {
        public RowInCompetitionTable(int position, FootballClub club, int gamesPlayed, int gamesWon, int gamesDrawn,
            int gamesLost, int goalsScored, int goalsMissed, int goalsDifference, int points)
        {
            Position = position;
            Club = club;
            GamesPlayed = gamesPlayed;
            GamesWon = gamesWon;
            GamesDrawn = gamesDrawn;
            GamesLost = gamesLost;
            GoalsScored = goalsScored;
            GoalsMissed = goalsMissed;
            GoalsDifference = goalsDifference;
            Points = points;
        }
        public int Position { get; private set; }
        public FootballClub Club { get; private set; }
        public int GamesPlayed { get; private set; }
        public int GamesWon { get; private set; }
        public int GamesDrawn { get; private set; }
        public int GamesLost { get; private set; }
        public int GoalsScored { get; private set; }
        public int GoalsMissed { get; private set; }
        public int GoalsDifference { get; private set; }
        public int Points { get; private set; }

    }
}
