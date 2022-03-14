using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Player : PersonDetails
    {
        public Player(string id, string firstName, string lastName, string fullName,
            DateTime? dateOfBirth, string citizenship, string placeOfBirth, 
            string club, int? numberInClub, string nationalTeam, int? numberInNatTeam,
            string position, string workingLeg, int? height, 
            int? weight) : base(id,firstName, lastName, fullName, dateOfBirth, citizenship, placeOfBirth)
        {
            Club = club;
            NumberInClub = numberInClub;
            NationalTeam = nationalTeam;
            NumberInNatTeam = numberInNatTeam;
            Position = position;
            WorkingLeg = workingLeg;
            Height = height;
            Weight = weight;
        }
        public string Club { get; private set; }
        public int? NumberInClub { get; private set; }
        public string NationalTeam { get; private set; }
        public int? NumberInNatTeam { get; private set; }
        public string Position { get; private set; }
        public string WorkingLeg { get; private set; }
        public int? Height { get; private set; }
        public int? Weight { get; private set; }
    }
}
