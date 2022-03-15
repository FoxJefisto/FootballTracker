using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Squad
    {
        public Squad(string id,List<Player> players)
        {
            Id = id;
            AllPlayers = players;
            Goalkeepers = new List<Player>();
            Defenders = new List<Player>();
            Midfielders = new List<Player>();
            Attackers = new List<Player>();
            foreach(var player in players)
            {
                switch (player.Position)
                {
                    case "вратарь":
                        Goalkeepers.Add(player);
                        break;
                    case "защитник":
                        Defenders.Add(player);
                        break;
                    case "полузащитник":
                        Midfielders.Add(player);
                        break;
                    case "нападающий":
                        Attackers.Add(player);
                        break;
                }
            }
        }
        public string Id { get; private set; }
        public List<Player> AllPlayers { get; private set; }
        public List<Player> Goalkeepers { get; private set; }
        public List<Player> Defenders { get; private set; }
        public List<Player> Midfielders { get; private set; }
        public List<Player> Attackers { get; private set; }
    }
}
