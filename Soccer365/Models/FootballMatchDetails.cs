using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballMatchDetails : FootballMatch
    {
        public FootballMatchDetails(FootballMatch match, MatchMainEvents mainEvents, Competitions competition, string stage,MatchSquads squads, 
            Pair<Person> coaches, MatchStatistics statistics, Stadiums stadium, int? attendance,
            List<Person> refereeTeam) : base(match.Id, match.Competition, match.Stage, match.Clubs, match.Status, match.Goals)
        {

            Stadium = stadium;
            Attendance = attendance;
            RefereeTeam = refereeTeam;
            Statistics = statistics;
            MainEvents = mainEvents;
            Squads = squads;
            Coaches = coaches;
        }
        public int? Attendance { get; private set; }
        public Stadiums Stadium { get; private set; }
        public List<Person> RefereeTeam { get; private set; }
        public MatchStatistics Statistics { get; private set; }
        public MatchMainEvents MainEvents { get; private set; }
        public MatchSquads Squads { get; private set; }
        public Pair<Person> Coaches { get; private set; }
    }
}
