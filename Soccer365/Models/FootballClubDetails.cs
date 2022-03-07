using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballClubDetails : FootballClub
    {
        public string FullName { get; private set; }
        public string City { get; private set; }
        public string Stadium { get; private set; }
        public DateTime FoundationDate { get; private set; }
        public int RatingUEFA { get; private set; }
        public List<ACompetition> Competitions { get; private set; }
        public List<FootballMatchDetails> MatchShedule { get; private set; }
        public List<FootballMatchDetails> MatchResults { get; private set; }

    }
}
