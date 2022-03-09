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
            Statistics statistics = soccer365Api.GetMatchStatistics("1675003");
            soccer365Api.PrintMatchStatistics(statistics);

        }
    }
}
