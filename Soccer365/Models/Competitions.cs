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
            Season = null;
        }
        public Competitions(string id, string name, string season, string currentStage = null)
        {
            Id = id;
            Name = name;
            Season = season;
            CurrentStage = currentStage;
        }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Season { get; private set; }
        public string CurrentStage { get; private set; }
    }
}
