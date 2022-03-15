using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Player : Person
    {
        public Player(FootballClub club,string id, string firstName, string lastName, int? number,
            string position, int? age, DateTime? dateOfBirth, string workingLeg,
            int? height, int? weight) : base(id, firstName, lastName)
        {
            Club = club;
            Number = number;
            Position = position;
            Age = age;
            DateOfBirth = dateOfBirth;
            WorkingLeg = workingLeg;
            Height = height;
            Weight = weight;

        }
        public FootballClub Club { get; private set; }
        public int? Number { get; private set; }
        public int? Age { get; private set; }
        public string Position { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string WorkingLeg { get; private set; }
        public int? Height { get; private set; }
        public int? Weight { get; private set; }
    }
}
