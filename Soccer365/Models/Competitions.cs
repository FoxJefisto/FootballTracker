using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Competitions
    {
        //todo: Доделать класс, добавить дополнительные поля о турнирах
        public Competitions(string name)
        {
            Name = name;
        }
        public Competitions(string id, string name)
        {
            Id = id;
            Name = name;

        }
        public Competitions(string id, string name, string country, string season = null)
        {
            Id = id;
            Name = name;
            Country = country;
            Season = season;
        }
        public string Id { get; private set; } = null;
        public string Name { get; private set; } = "";
        public string Season { get; set; } = null;
        public string Country { get; private set; } = "";
    }
}
