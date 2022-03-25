using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class RowCompetitionPlayerStatistics : RowSquadPlayerStatistics
    {
        public RowCompetitionPlayerStatistics(FootballClub club, Person player, int? number, string position, int goals, int assists, 
            int matches, int minutes, int goalPlusPass, int penGoals, int doubleGoals, int hatTricks, int autoGoals, int yellowCards, int yellowRedCards, 
            int redCards, int fairPlayScore) : base(player, number, position, goals, assists,
            matches, minutes, goalPlusPass, penGoals, doubleGoals, hatTricks, autoGoals, yellowCards, yellowRedCards,
            redCards, fairPlayScore)
        {
            Club = club;
        }

        public FootballClub Club { get; private set; }
    }
}
