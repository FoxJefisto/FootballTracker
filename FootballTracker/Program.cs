using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Rest;
using Soccer365;

namespace FootballTracker
{
    enum SearchScope{
        coaches,
        players,
        clubs
    }
    interface IFootballAPI
    {
        string GetHTMLTodayMatches();
        void PrintTodayMatches(object obj);
        string GetHTMLSearch(string value);

        int GetClubId(string club);
        int GetCoachId(string coach);
        int GetPlayerId(string player);

        void Search(string value, params SearchScope[] scope);
        string GetHTMLClubInfo(int clubId);

        void PrintClubInfo(int clubId);
        void PrintClubInfo(string clubName);

        

        //void SaveClubInfo(int clubId);
        //void SaveClubInfo(string clubName);
    }

    class FootballAPI : IFootballAPI
    {
        private readonly RestClient restClient = new RestClient();
        private readonly Soccer365Api api = new Soccer365Api();

        public void PrintTodayMatches(object obj)
        {
            Console.Clear();
            Console.WriteLine(DateTime.Now.ToString("F"));
            Console.WriteLine(new string('-',25));

            var matches = api.GetTodayMatches();
            matches.ForEach(
                match => Console.WriteLine($"{match.StartDate,-20}{$"{match.ClubHome} - {match.ClubAway}",50}{match.Score,10}")
            );
        }

        public string GetHTMLTodayMatches()
        {
            return restClient.GetStringAsync("https://soccer365.ru/online/").Result;
        }

        public string GetHTMLSearch(string value)
        {
            return restClient.GetStringAsync("https://soccer365.ru/?a=search&q=" + HttpUtility.UrlEncode(value)).Result;
        }

        public void Search(string value, params SearchScope[] scope)
        {
            Console.Clear();
            string htmlCode = GetHTMLSearch(value);
            string scopesFilter;
            if (scope.Length == 0)
            {
                scopesFilter = "coaches | players | clubs";
            }
            else
            {
                scopesFilter = scope[0].ToString();
                for (int i = 1; i < scope.Length; i++)
                {
                    scopesFilter = " | " + scope[i].ToString();
                }
            }
            string patternClub = string.Format(@"href=""\/[{0}]+\/([0-9]+)\/"">([0-9А-Яа-я \-]+)<\/a>",scopesFilter);
            Regex regexClub = new Regex(patternClub);
            Match matchClub = regexClub.Match(htmlCode);
            while (matchClub.Success)
            {
                Console.WriteLine($"{matchClub.Groups[1].Value,-20}{matchClub.Groups[2].Value,20}");
                matchClub = matchClub.NextMatch();
            }
        }

        public int GetClubId(string club)
        {
            string htmlCode = GetHTMLSearch(club);
            string patternClubId = @"href=""\/clubs\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return int.Parse(matchClubId.Groups[1].Value);
        }

        public int GetCoachId(string coach)
        {
            string htmlCode = GetHTMLSearch(coach);
            string patternClubId = @"href=""\/coaches\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return int.Parse(matchClubId.Groups[1].Value);
        }

        public int GetPlayerId(string player)
        {
            string htmlCode = GetHTMLSearch(player);
            string patternClubId = @"href=""\/players\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return int.Parse(matchClubId.Groups[1].Value);
        }

        public string GetHTMLClubInfo(int clubId)
        {
            return restClient.GetStringAsync($"https://soccer365.ru/clubs/{clubId}/").Result;
        }

        public void PrintClubInfo(int clubId)
        {
            Console.Clear();
            string htmlCode = GetHTMLClubInfo(clubId);
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo,indexEndInfo-indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            if (matchTitle.Success)
            {
                Console.WriteLine($"{matchTitle.Groups[1].Value}\n{matchTitle.Groups[2].Value}");
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);
                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    Console.WriteLine($"{matchKeyInfo.Groups[1].Value, -20}{matchValueInfo.Groups[1].Value,-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

        public void PrintClubInfo(string clubName)
        {
            Console.Clear();
            int clubId = GetClubId(clubName);
            string htmlCode = GetHTMLClubInfo(clubId);
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            if (matchTitle.Success)
            {
                Console.WriteLine($"{matchTitle.Groups[1].Value}\n{matchTitle.Groups[2].Value}");
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);
                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    if(matchKeyInfo.Groups[1].Value == "Стадион")
                    {
                        Console.WriteLine($"{matchKeyInfo.Groups[1].Value,-20}{matchValueInfo.Groups[1].Value,-30}");
                        matchValueInfo = matchValueInfo.NextMatch();
                        Console.WriteLine($"Город-страна{"",8}{matchValueInfo.Groups[1].Value,-30}");
                    }
                    else
                        Console.WriteLine($"{matchKeyInfo.Groups[1].Value,-20}{matchValueInfo.Groups[1].Value,-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            FootballAPI footballAPI = new FootballAPI();
            //footballAPI.Search(Console.ReadLine());


            #region PrintTodayMatches
            while (true)
            {
                TimerCallback tm = new TimerCallback(footballAPI.PrintTodayMatches);
                Timer timer = new Timer(tm, null, 0, 30000);
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    timer.Dispose();
                    return;
                }
            }
            #endregion

            //footballAPI.PrintClubInfo(Console.ReadLine());
        }
    }
}
