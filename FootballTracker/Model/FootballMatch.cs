using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTracker
{
    class FootballMatch
    {
        public string MatchId { get; private set; }
        public FootballClub ClubHome { get; private set; }
        public FootballClub ClubAway { get; private set; }
        public DateTime StartDate { get; private set; }
        public FootballClub Winner { get; private set; }
        public string MatchScore { get; private set; }
    }
}
