namespace Soccer365.Models
{
    public struct MatchStatisticsStruct
    {
        public Pair<float> xg;
        public Pair<int> shots;
        public Pair<int> shotsOnTarget;
        public Pair<int> shotsBlocked;
        public Pair<int> saves;
        public Pair<int> ballPossession;
        public Pair<int> corners;
        public Pair<int> fouls;
        public Pair<int> offsides;
        public Pair<int> yCards;
        public Pair<int> rCards;
        public Pair<int> attacks;
        public Pair<int> attacksDangerous;
        public Pair<int> passes;
        public Pair<float> accPasses;
        public Pair<int> freeKicks;
        public Pair<int> prowing;
        public Pair<int> crosses;
        public Pair<int> tackles;
    }
    public class MatchStatistics
    {
        public MatchStatistics(string id, MatchStatisticsStruct statistics)
        {
            Id = id;
            Xg = statistics.xg;
            Shots = statistics.shots;
            ShotsOnTarget = statistics.shotsOnTarget;
            ShotsBlocked = statistics.shotsBlocked;
            Saves = statistics.saves;
            BallPossession = statistics.ballPossession;
            Corners = statistics.corners;
            Fouls = statistics.fouls;
            Offsides = statistics.offsides;
            YCards = statistics.yCards;
            RCards = statistics.rCards;
            Attacks = statistics.attacks;
            AttacksDangerous = statistics.attacksDangerous;
            Passes = statistics.passes;
            AccPasses = statistics.accPasses;
            FreeKicks = statistics.freeKicks;
            Prowing = statistics.prowing;
            Crosses = statistics.crosses;
            Tackles = statistics.tackles;
        }
        public string Id { get; private set; }
        public Pair<float> Xg { get; private set; }
        public Pair<int> Shots { get; private set; }
        public Pair<int> ShotsOnTarget { get; private set; }
        public Pair<int> ShotsBlocked { get; private set; }
        public Pair<int> Saves { get; private set; }
        public Pair<int> BallPossession { get; private set; }
        public Pair<int> Corners { get; private set; }
        public Pair<int> Fouls { get; private set; }
        public Pair<int> Offsides { get; private set; }
        public Pair<int> YCards { get; private set;  }
        public Pair<int> RCards { get; private set; }
        public Pair<int> Attacks { get; private set; }
        public Pair<int> AttacksDangerous { get; private set; }
        public Pair<int> Passes { get; private set; }
        public Pair<float> AccPasses { get; private set; }
        public Pair<int> FreeKicks { get; private set; }
        public Pair<int> Prowing { get; private set; }
        public Pair<int> Crosses { get; private set; }
        public Pair<int> Tackles { get; private set; }
    }
}
