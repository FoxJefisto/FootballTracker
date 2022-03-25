namespace Soccer365.Models
{
    public class Stadiums
    {
        public Stadiums(string id, string name, string city, string country, string temp, string weather)
        {
            Id = id;
            Name = name;
            City = city;
            Country = country;
            Temp = temp;
            Weather = weather;
        }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string City { get; private set; }
        public string Country { get; private set; }
        public string Temp { get; private set; }
        public string Weather { get; private set; }

    }
}
