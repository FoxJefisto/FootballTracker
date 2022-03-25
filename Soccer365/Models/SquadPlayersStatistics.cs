using System.Collections.Generic;

namespace Soccer365.Models
{
    public class SquadPlayersStatistics
    {
        public SquadPlayersStatistics(string id, string season, Competitions competition, List<RowSquadPlayerStatistics> playerStatistics)
        {
            Id = id;
            Season = season;
            Competition = competition;
            PlayerStatistics = playerStatistics;
        }

        public string Id { get; private set; }
        public string Season { get; private set; }
        public Competitions Competition { get; private set; }
        public List<RowSquadPlayerStatistics> PlayerStatistics { get; private set; }
    }
}
