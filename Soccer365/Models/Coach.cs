using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Coach : Person
    {
        public int CoachId { get; private set; }
        public string Club { get; private set; }
        public string Biography { get; private set; }
    }
}
