using System;

namespace Soccer365.Models
{
    public enum EventsName{
        pengoal,
        goal,
        yellowcard,
        redcard,
        yellowrred
    }
    public enum TeamType
    {
        Home,
        Away
    }
    public class MatchMainEvent : IComparable<MatchMainEvent>
    {
        public MatchMainEvent(TeamType team,int minute, string eventName, Person mainAuthor, Person secondAuthor = null)
        {
            Team = team;
            Minute = minute;
            Enum.TryParse(eventName,out EventsName name);
            Name = name;
            MainAuthor = mainAuthor;
            SecondAuthor = secondAuthor;
        }
        public Person MainAuthor { get; private set; }
        public Person SecondAuthor { get; private set; }
        public EventsName Name { get; private set; }
        public int Minute { get; private set;  }
        public TeamType Team { get; private set; }

        public int CompareTo(MatchMainEvent x)
        {
            if (x is null) throw new ArgumentException("Некорректное значение параметра");
            return Minute.CompareTo(x.Minute);
        }
    }
}
