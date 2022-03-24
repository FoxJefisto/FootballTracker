using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionPlayersStatistics
    {
        public CompetitionPlayersStatistics(string id, string season, List<RowSquadPlayerStatistics> playerStatistics)
        {
            Id = id;
            Season = season;
            PlayerStatistics = playerStatistics;
        }

        public string Id { get; private set; }
        public string Season { get; private set; }
        public List<RowSquadPlayerStatistics> PlayerStatistics { get; private set; }
    }
}
