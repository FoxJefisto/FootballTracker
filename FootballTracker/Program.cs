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
            ListOfMatches listOfMatches = soccer365Api.GetMatchesByDate(DateTime.Parse(Console.ReadLine()));
            soccer365Api.PrintMatches(listOfMatches);
            FootballClubDetails club = soccer365Api.GetClubInfoByName(Console.ReadLine());
            soccer365Api.PrintClubInfo(club);
            FootballPlayer player = soccer365Api.GetPlayerInfoByName(Console.ReadLine());
            soccer365Api.PrintPlayerInfo(player);
            Coach coach = soccer365Api.GetCoachInfoByName(Console.ReadLine());
            soccer365Api.PrintCoachInfo(coach);

        }
    }
}
