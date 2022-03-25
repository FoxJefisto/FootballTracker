using System.Collections.Generic;

namespace Soccer365.Models
{
    public class CompetitionSingleTable
    {
        public CompetitionSingleTable() {
            Id = null;
            Season = null;
            Table = new List<RowInCompetitionTable>();
        }
        public CompetitionSingleTable(string id, string season, List<RowInCompetitionTable> table)
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
