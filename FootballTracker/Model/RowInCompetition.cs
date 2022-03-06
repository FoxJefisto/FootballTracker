using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTracker
{
    class RowInCompetition
    {
        public int Position { get; private set; }
        public FootballClubDetails FootballClub { get; private set; }
        public int GamesPlayed { get; private set;  }
        public int GamesWon { get; private set; }
        public int GamesDrawn { get; private set; }
        public int GamesLost { get; private set; }
        public int GoalsScored { get; private set; }
        public int GoalsMissed { get; private set; }
        public int GoalsDifference { get; private set; }
        public int Points { get; private set; }

    }
}
