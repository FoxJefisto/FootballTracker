using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Soccer365;
using Soccer365.Models;

namespace FootballTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            Soccer365Api soccer365Api = new Soccer365Api();
            var statistics = soccer365Api.GetMatchStatistics("1675010");
            var squads = soccer365Api.GetMatchSquads("1675010");
            var events = soccer365Api.GetMatchMainEvents("1675010");
            soccer365Api.PrintAllInfoMatch(events, squads, statistics);
        }
    }
}
