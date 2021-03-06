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
            string result = $"{HomeTeam,19}{"",14}{AwayTeam,-40}";
            return result;
        }
    }
}
