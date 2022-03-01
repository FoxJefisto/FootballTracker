using System;
using System.Net;
using System.Text.RegularExpressions;

namespace FootballTracker
{
    interface IFootballAPI
    {
        string GetTodayMatchesHTML();
        FootballMatch PrintTodayMatches();
    }

    class FootballAPI : IFootballAPI
    {
        public FootballMatch PrintTodayMatches()
        {
            throw new NotImplementedException();
        }

        public string GetTodayMatchesHTML()
        {
            WebProxy proxy = new WebProxy("127.0.0.1:8888");
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

            getRequest.Proxy = proxy;
            getRequest.Run(cookieContainer);

            return getRequest.Response;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            FootballAPI footballAPI = new FootballAPI();
            string htmlCode = footballAPI.GetTodayMatchesHTML();
            string pattern = @"title=""([^""]* - [^""]*)"">";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(htmlCode);
            while (match.Success)
            {
                // Т.к. мы выделили в шаблоне одну группу (одни круглые скобки),
                // ссылаемся на найденное значение через свойство Groups класса Match
                Console.WriteLine(match.Groups[1].Value);

                // Переходим к следующему совпадению
                match = match.NextMatch();
            }
        }
    }
}
