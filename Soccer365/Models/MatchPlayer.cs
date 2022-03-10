using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class MatchPlayer
    {
        public MatchPlayer(int number, Person player, string info = null)
        {
            Number = number;
            Player = player;
            Info = info;
        }
        public int Number { get; private set; }
        public Person Player { get; private set; }
        public string Info { get; private set; }
    }
}
