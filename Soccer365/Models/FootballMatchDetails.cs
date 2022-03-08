using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballMatchDetails : FootballMatch
    {
        public Competition Competition { get; private set; }
        public string Stage { get; private set; }

        public string City { get; private set; }
        public string Country { get; private set; }
        public string Stadium { get; private set; }
        public int Attendance { get; private set; }
        public List<Person> RefereeTeam { get; private set; }

    }
}
