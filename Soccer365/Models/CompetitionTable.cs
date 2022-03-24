using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionTable
    {
        public CompetitionTable()
        {
            Name = default;
            Table = new List<RowInCompetitionTable>();
        }
        public CompetitionTable(char name, List<RowInCompetitionTable> table)
        {
            Name = name;
            Table = table;
        }
        public char Name { get; private set; }
        public List<RowInCompetitionTable> Table { get; private set; }
    }
}
