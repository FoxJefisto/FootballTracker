using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTracker
{
    abstract class ACompetition
    {
        public string Country { get; private set; }
        public DateTime DateStart { get; private set; }
        public DateTime DateEnd { get; private set; }
        public int Attendance { get; private set; }
        public string CurrentStage { get; private set; }
    }
}
