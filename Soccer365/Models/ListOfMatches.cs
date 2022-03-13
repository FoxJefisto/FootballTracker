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
            Matches = footballMatches;
        }
        public DateTime Date { get; private set; }
        public List<FootballMatch> Matches=null;
    }
}
