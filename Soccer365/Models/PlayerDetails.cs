using System;

namespace Soccer365.Models
{
    public class PlayerDetails : PersonDetails
    {
        public PlayerDetails(string id, string firstName, string lastName, string fullName,
            DateTime? dateOfBirth, string citizenship, string placeOfBirth, 
            FootballClub club, int? numberInClub, FootballClub nationalTeam, int? numberInNatTeam,
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
        public FootballClub Club { get; private set; }
        public int? NumberInClub { get; private set; }
        public FootballClub NationalTeam { get; private set; }
        public int? NumberInNatTeam { get; private set; }
        public string Position { get; private set; }
        public string WorkingLeg { get; private set; }
        public int? Height { get; private set; }
        public int? Weight { get; private set; }
    }
}
