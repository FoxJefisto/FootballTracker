using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTracker
{
    class FootballMatchDetails : FootballMatch
    {
        public ACompetition Competition { get; private set; }
        public string Stage { get; private set; }
        public DateTime StartDate { get; private set; }
        public string City { get; private set; }
        public string Country { get; private set; }
        public string Stadium { get; private set; }
        public int Attendance { get; private set; }
        public List<Person> RefereeTeam { get; private set; }

    }
}
