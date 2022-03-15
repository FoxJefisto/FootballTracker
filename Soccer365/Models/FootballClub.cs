using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballClub
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public FootballClub(){}

        public FootballClub(string name)
        {
            Name = name;
            Id = null;
        }
        public FootballClub(string clubId, string name)
        {
            Id = clubId;
            Name = name;
        }
    }
}
