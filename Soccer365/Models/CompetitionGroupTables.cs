using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionGroupTables
    {
        public CompetitionGroupTables()
        {
            Id = null;
            Season = null;
            GroupTable = new List<CompetitionTable>();
            
        }
        public CompetitionGroupTables(string id, string season, List<CompetitionTable> table)
        {
            Id = id;
            Season = season;
            GroupTable = table;
        }
        public string Id { get; private set; }
        public string Season { get; private set; }
        public List<CompetitionTable> GroupTable { get; private set; }
    }
}
