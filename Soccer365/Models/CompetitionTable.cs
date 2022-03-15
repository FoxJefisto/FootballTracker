using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionTable
    {
        public CompetitionTable() {
            Id = null;
            Season = null;
            Table = new List<RowInCompetitionTable>();
        }
        public CompetitionTable(string id, string season, List<RowInCompetitionTable> table)
        {
            Id = id;
            Season = season;
            Table = table;
        }
        public string Id { get; private set; }
        public string Season { get; private set; }
        public List<RowInCompetitionTable> Table { get; private set; }
    }
}
