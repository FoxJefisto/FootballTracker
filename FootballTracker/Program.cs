using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace FootballTracker
{
    interface IFootballAPI
    {
        string GetTodayMatchesHTML();
        void PrintTodayMatches(object obj);
    }

    class FootballAPI : IFootballAPI
    {
        public void PrintTodayMatches(object obj)
        {
            Console.Clear();
            string htmlCode = GetTodayMatchesHTML();
            string patternClubs = @"title=""([^""]* - [^""]*)"">";
            string patternStartDate = @"""status"".*>([0-9А-Яа-я':., ]+)<.*\/div>";
            string patternScore = @"<div class=""gls"">([0-9\-]+)<\/div>";
            Regex regexClubs = new Regex(patternClubs);
            Regex regexStartDate = new Regex(patternStartDate);
            Regex regexScore = new Regex(patternScore);
            Match matchClubs = regexClubs.Match(htmlCode);
            Match matchStartDate = regexStartDate.Match(htmlCode);
            Match matchScore = regexScore.Match(htmlCode);
            Console.WriteLine(DateTime.Now.ToString("F"));
            Console.WriteLine(new string('-',25));
            while (matchClubs.Success || matchStartDate.Success || matchScore.Success)
            {
                Console.Write($"{matchStartDate.Groups[1].Value,-20}{matchClubs.Groups[1].Value,40}{matchScore.Groups[1].Value,10}");
                matchScore = matchScore.NextMatch();
                Console.WriteLine($":{matchScore.Groups[1].Value}");
                matchScore = matchScore.NextMatch();
                // Переходим к следующему совпадению
                matchClubs = matchClubs.NextMatch();
                matchStartDate = matchStartDate.NextMatch();
            }
        }

        public string GetTodayMatchesHTML()
        {
            //WebProxy proxy = new WebProxy("127.0.0.1:8888");
            CookieContainer cookieContainer = new CookieContainer();
            GetRequest getRequest = new GetRequest("https://soccer365.ru/online/");
            getRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.174 YaBrowser/22.1.3.848 Yowser/2.5 Safari/537.36";
            getRequest.Referer = "https://soccer365.ru/";
            getRequest.Host = "soccer365.ru";

            getRequest.Headers.Add("sec-ch-ua", "\"Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Yandex\"; v=\"22\"");
            getRequest.Headers.Add("sec-ch-ua-mobile", "?0");
            getRequest.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            getRequest.Headers.Add("Sec-Fetch-Dest", "document");
            getRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
            getRequest.Headers.Add("Sec-Fetch-Site", "same-origin");
            getRequest.Headers.Add("Sec-Fetch-User", "?1");
            getRequest.Headers.Add("Upgrade-Insecure-Requests", "1");

            //getRequest.Proxy = proxy;
            getRequest.Run(cookieContainer);

            return getRequest.Response;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                FootballAPI footballAPI = new FootballAPI();
                TimerCallback tm = new TimerCallback(footballAPI.PrintTodayMatches);
                Timer timer = new Timer(tm,null, 0, 30000);
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    timer.Dispose();
                    return;
                }
            }
        }
    }
}
