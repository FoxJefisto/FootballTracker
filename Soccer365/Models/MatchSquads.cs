using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class MatchSquads
    {
        public MatchSquads(Pair<List<MatchPlayer>> startSquad, Pair<List<MatchPlayer>> reservePlayers, Pair<List<MatchPlayer>> missingPlayers = null)
        {
            StartSquad = startSquad;
            ReservePlayers = reservePlayers;
            MissingPlayers = missingPlayers;
        }
        public Pair<List<MatchPlayer>> StartSquad { get; private set; }
        public Pair<List<MatchPlayer>> ReservePlayers { get; private set; }
        //TODO: доделать поддержку игроков пропускающих матч
        public Pair<List<MatchPlayer>> MissingPlayers { get; private set; }
    }
}
