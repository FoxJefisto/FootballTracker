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
            Id = null;
            Name = name;
            CurrentStage = null;
            Country = null;
            Season = null;
        }
        public Competitions(string id, string name)
        {
            Id = id;
            Name = name;
            CurrentStage = null;
            Country = null;
            Season = null;
        }
        public Competitions(string id, string name, string season, string country, string currentStage = null)
        {
            Id = id;
            Name = name;
            Season = season;
            Country = country;
            CurrentStage = currentStage;
        }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Season { get; private set; }
        public string CurrentStage { get; private set; }
        public string Country { get; private set; }
    }
}
