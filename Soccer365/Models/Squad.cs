﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Squad
    {
        public List<Footballer> Goalkeepers { get; private set; }
        public List<Footballer> Defenders { get; private set; }
        public List<Footballer> Midfielders { get; private set; }
        public List<Footballer> Attackers { get; private set; }
    }
}