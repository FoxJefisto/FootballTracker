using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Coach : Person
    {
        public Coach(string firstName, string lastName, string fullName, 
            DateTime? dateOfBirth, string citizenship, string placeOfBirth,
            string id, string club, int? height, int? weight) : base(firstName, lastName, fullName, dateOfBirth, citizenship, placeOfBirth)
        {
            Id = id;
            Club = club;
            Height = height;
            Weight = weight;
        }
        public string Id { get; private set; }
        public string Club { get; private set; }
        public int? Height { get; private set; }
        public int? Weight { get; private set; }
    }
}
