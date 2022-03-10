using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Coach : PersonDetails
    {
        public Coach(string id, string firstName, string lastName, string fullName, 
            DateTime? dateOfBirth, string citizenship, string placeOfBirth,
            string club, int? height, int? weight) : base(id, firstName, lastName, fullName, dateOfBirth, citizenship, placeOfBirth)
        {
            Club = club;
            Height = height;
            Weight = weight;
        }
        public string Club { get; private set; }
        public int? Height { get; private set; }
        public int? Weight { get; private set; }
    }
}
