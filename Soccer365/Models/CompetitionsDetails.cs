using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionsDetails : Competitions
    {
        public CompetitionsDetails(string id, string name, string currentStage, string country, DateTime dateStart, DateTime dateEnd, int? attendance) : base(id, name, currentStage)
        {
            Country = country;
            DateStart = dateStart;
            DateEnd = dateEnd;
            Attendance = attendance;
        }
        public string Country { get; private set; }
        public DateTime DateStart { get; private set; }
        public DateTime DateEnd { get; private set; }
        public int? Attendance { get; private set; }
    }
}
