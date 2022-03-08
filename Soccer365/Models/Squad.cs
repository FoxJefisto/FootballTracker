using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Squad
    {
        public List<FootballPlayer> Goalkeepers { get; private set; }
        public List<FootballPlayer> Defenders { get; private set; }
        public List<FootballPlayer> Midfielders { get; private set; }
        public List<FootballPlayer> Attackers { get; private set; }
    }
}
