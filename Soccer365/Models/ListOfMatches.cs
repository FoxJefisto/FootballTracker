using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class ListOfMatches
    {
        public ListOfMatches(DateTime date, List<FootballMatch> footballMatches)
        {
            Date = date;
            this.footballMatches = footballMatches;
        }
        public DateTime Date { get; private set; }
        public List<FootballMatch> footballMatches=null;
    }
}
