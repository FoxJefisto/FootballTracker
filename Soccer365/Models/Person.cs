using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Person
    {
        public Person(string firstName, string lastName, string fullName, DateTime? dateOfBirth, string citizenship, string placeOfBirth)
        {
            FirstName = firstName;
            LastName = lastName;
            FullName = fullName;
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
            DateTime date = (DateTime) dateOfBirth;
            int age = now.Year - date.Year - 1;
            if (now.DayOfYear < date.DayOfYear)
                age++;
            return age;
        }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName { get; private set; }
        public int? Age { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string Citizenship { get; private set; }
        public string PlaceOfBirth { get; private set; }
    }
}
