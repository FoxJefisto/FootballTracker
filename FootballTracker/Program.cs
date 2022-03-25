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
            var table = soccer365Api.GetMatchesByDate(DateTime.Parse("20.03.2022"), "премьер-лига");
            soccer365Api.PrintMatches(table);
        }
    }
}
