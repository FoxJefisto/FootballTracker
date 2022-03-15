using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class PersonDetails : Person
    {
        public PersonDetails(string id,string firstName, string lastName, string fullName, DateTime? dateOfBirth, string citizenship, string placeOfBirth) : base(id,firstName,lastName)
        {
            OriginalName = fullName;
            DateOfBirth = dateOfBirth;
            Age = GetAge(dateOfBirth);
            Citizenship = citizenship;
            PlaceOfBirth = placeOfBirth;
        }
        private int? GetAge(DateTime? dateOfBirth)
        {
            if (dateOfBirth == null)
                return null;
            DateTime now = DateTime.Now;
            DateTime date = (DateTime)dateOfBirth;
            int age = now.Year - date.Year - 1;
            if (now.DayOfYear < date.DayOfYear)
                age++;
            return age;
        }
        public string OriginalName { get; private set; }
        public int? Age { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string Citizenship { get; private set; }
        public string PlaceOfBirth { get; private set; }
    }
}
