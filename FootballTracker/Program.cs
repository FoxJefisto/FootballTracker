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
            //Console.WriteLine("Укажите дату:");
            //DateTime date = DateTime.Parse(Console.ReadLine());
            //Console.WriteLine("Введите название команды: ");
            //var match = soccer365Api.GetMatchByDateName(date, Console.ReadLine());
            //var matchInfo = soccer365Api.GetMatchAllInfo(match.Id);
            //soccer365Api.PrintMatchAllInfo(matchInfo);
            soccer365Api.PrintMatch(soccer365Api.GetMatchById("1709865"));
        }
    }
}
