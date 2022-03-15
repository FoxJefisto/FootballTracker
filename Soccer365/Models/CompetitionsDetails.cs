using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionsDetails : Competitions
    {
        public CompetitionsDetails(string id, string name, string season, string currentStage, string country, DateTime? dateStart, DateTime? dateEnd, int? attendance, int? matchesPlayed, int? matchesAll) : base(id, name, season, currentStage)
        {
            Country = country;
            DateStart = dateStart;
            DateEnd = dateEnd;
            Attendance = attendance;
            MatchesPlayed = matchesPlayed;
            MatchesAll = matchesAll;
        }
        public string Country { get; private set; }
        public DateTime? DateStart { get; private set; }
        public DateTime? DateEnd { get; private set; }
        public int? Attendance { get; private set; }
        public int? MatchesPlayed { get; private set; }
        public int? MatchesAll { get; private set; }
    }
}
