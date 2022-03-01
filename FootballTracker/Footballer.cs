using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTracker
{
    class Footballer : Person
    {
        public int PlayerId { get; private set; }
        public string Club { get; private set; }
        public int Number { get; private set; }
        public string Position { get; private set; }
        public string WorkingLeg { get; private set; }
        public int Height { get; private set; }
        public int Weight { get; private set; }
    }
}
