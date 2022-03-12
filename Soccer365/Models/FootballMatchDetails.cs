using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public struct MatchDetailsStruct
    {
        public string matchId;
        public Pair<FootballClub> clubs;
        public string matchStatus;
        public Pair<int?> goals;
        public Competitions competition;
        public string stage;
        public Stadiums stadium;
        public int? attendance;
        public List<Person> refereeTeam;
        public MatchStatistics statistics;
        public MatchMainEvents mainEvents;
        public MatchSquads squads;
        public Pair<Person> coaches;
    }
    public class FootballMatchDetails : FootballMatch
    {
        public FootballMatchDetails(MatchDetailsStruct st) : base(st.matchId, st.clubs, st.matchStatus, st.goals)
        {
            Competition = st.competition;
            Stage = st.stage;
            Stadium = Stadium;
            Attendance = st.attendance;
            RefereeTeam = st.refereeTeam;
            Statistics = st.statistics;
            MainEvents = st.mainEvents;
            Squads = st.squads;
            Coaches = st.coaches;
        }
        public Competitions Competition { get; private set; }
        public string Stage { get; private set; }
        public int? Attendance { get; private set; }
        public Stadiums Stadium { get; private set; }
        public List<Person> RefereeTeam { get; private set; }
        public MatchStatistics Statistics { get; private set; }
        public MatchMainEvents MainEvents { get; private set; }
        public MatchSquads Squads { get; private set; }
        public Pair<Person> Coaches { get; private set; }
    }
}
