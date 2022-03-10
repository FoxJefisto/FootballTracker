using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soccer365.Models
{
    public class MatchMainEvents
    {
        public MatchMainEvents(string id,List<MatchMainEvent> eventsHome, List<MatchMainEvent> eventsAway)
        {
            Id = id;
            EventsHome = eventsHome;
            EventsAway = eventsAway;
            Events = getEvents(eventsHome, eventsAway);
        }

        private List<MatchMainEvent> getEvents(List<MatchMainEvent> eventsHome, List<MatchMainEvent> eventsAway)
        {
            List<MatchMainEvent> events = new List<MatchMainEvent>();
            events.AddRange(eventsHome);
            events.AddRange(eventsAway);
            events.Sort();
            return events;
        }
        public string Id { get; private set; }
        public List<MatchMainEvent> EventsHome { get; private set;  }
        public List<MatchMainEvent> EventsAway { get; private set;  }
        public List<MatchMainEvent> Events { get; private set; }
    }
}
