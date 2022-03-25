using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionPlayersStatistics
    {
        public CompetitionPlayersStatistics(Competitions competition, List<RowCompetitionPlayerStatistics> playerStatistics)
        {
            Competition = competition;
            PlayerStatistics = playerStatistics;
        }
        public Competitions Competition { get; private set; }
        public List<RowCompetitionPlayerStatistics> PlayerStatistics { get; private set; }
    }
}
