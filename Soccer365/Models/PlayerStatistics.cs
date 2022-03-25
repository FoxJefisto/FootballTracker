using System.Collections.Generic;

namespace Soccer365.Models
{
    public class PlayerStatistics
    {
        public PlayerStatistics(string playerId, List<RowPlayerStatistics> statistics)
        {
            PlayerId = playerId;
            Statistics = statistics;
        }
        public string PlayerId { get; private set;  }
        public List<RowPlayerStatistics> Statistics { get; private set;  }
    }
}
