using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class Pair<T>
    {
        public Pair(T homeTeam, T awayTeam)
        {
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
        }
        public T HomeTeam { get; private set; }
        public T AwayTeam { get; private set; }
        public string OutPair()
        {
            string result = $"{HomeTeam,-5} - {AwayTeam,5}";
            return result;
        }
    }
}
