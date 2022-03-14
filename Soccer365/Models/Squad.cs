using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Squad
    {
        public List<Player> Goalkeepers { get; private set; }
        public List<Player> Defenders { get; private set; }
        public List<Player> Midfielders { get; private set; }
        public List<Player> Attackers { get; private set; }
    }
}
