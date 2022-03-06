using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTracker
{
    class FootballClub
    {
        public int ClubId { get; private set; }
        public string Name { get; private set; }
        public FootballClub()
        {

        }
        public FootballClub(int clubId, string name)
        {
            this.ClubId = ClubId;
            this.Name = name;
        }
    }
}
