using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballClub
    {
        public int? ClubId { get; private set; }
        public string Name { get; private set; }
        public FootballClub()
        {

        }

        public FootballClub(string name)
        {
            Name = name;
            ClubId = null;
        }
        public FootballClub(int clubId, string name)
        {
            ClubId = clubId;
            Name = name;
        }
    }
}
