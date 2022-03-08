using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Competition
    {
        //todo: Доделать класс, добавить дополнительные поля о турнирах
        public Competition(string name)
        {
            Id = null;
            Name = name;
            Country = null;
            DateStart = default;
            DateEnd = default;
            Attendance = 0;
            CurrentStage = null;
        }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Country { get; private set; }
        public DateTime DateStart { get; private set; }
        public DateTime DateEnd { get; private set; }
        public int Attendance { get; private set; }
        public string CurrentStage { get; private set; }
    }
}
