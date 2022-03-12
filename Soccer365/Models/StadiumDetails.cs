using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class StadiumDetails : Stadiums
    {
        public StadiumDetails(string id, string name, string city, string country, string temp, string weather,
            string capacity, string dateOfOpening, string club) : base(id, name,city, country, temp, weather)
        {
            Capacity = capacity;
            DateOfOpening = dateOfOpening;
            Club = club;
        }
        public string Capacity { get; private set; }
        public string DateOfOpening { get; private set; }
        public string Club { get; private set; }
    }
}
