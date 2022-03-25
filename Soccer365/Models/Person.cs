namespace Soccer365.Models
{
    public class Person
    {
        public Person(string id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Name = firstName + " " + lastName;
        }
        public Person(string firstName, string lastName)
        {
            Id = null;
            FirstName = firstName;
            LastName = lastName;
            Name = firstName + " " + lastName;
        }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Id { get; private set; }
        public string Name { get; private set; }
        
    }
}
