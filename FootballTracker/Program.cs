using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace FootballTracker
{
    enum SearchScope{
        coaches,
        players,
        clubs,
        games
    }
    interface IFootballAPI
    {
        string GetHTMLTodayGames();
        string GetHTMLSearch(string value);
        string GetHTMLInfo(int clubId, SearchScope scope);

        int GetClubId(string club);
        int GetCoachId(string coach);
        int GetPlayerId(string player);

        void Search(string value, params SearchScope[] scope);

        void PrintTodayGames(object obj);
        void PrintClubInfo(int clubId);
        void PrintClubInfo(string clubName);

        void PrintPlayerInfo(int playerId);
        void PrintPlayerInfo(string playerName);

        void PrintCoachInfo(int coachId);
        void PrintCoachInfo(string coachName);

        void PrintGameInfo(int gameId);
    }

    class FootballAPI : IFootballAPI
    {
        public string GetHTMLTodayGames()
        {
            CookieContainer cookieContainer = new CookieContainer();
            GetRequest getRequest = new GetRequest("https://soccer365.ru/online/");
            SavedRequestHeaders.GetHeaders1(getRequest);

            WebProxy proxy = new WebProxy("127.0.0.1:8888");
            getRequest.Proxy = proxy;

            getRequest.Run(cookieContainer);

            return getRequest.Response;
        }
        public string GetHTMLSearch(string value)
        {

            CookieContainer cookieContainer = new CookieContainer();
            string address = "https://soccer365.ru/?a=search&q=" + HttpUtility.UrlEncode(value);
            GetRequest getRequest = new GetRequest(address);
            SavedRequestHeaders.GetHeaders1(getRequest);

            //WebProxy proxy = new WebProxy("127.0.0.1:8888");
            //getRequest.Proxy = proxy;

            getRequest.Run(cookieContainer);
            return getRequest.Response;
        }
        public string GetHTMLInfo(int clubId, SearchScope scope)
        {
            CookieContainer cookieContainer = new CookieContainer();
            string address = $"https://soccer365.ru/{scope.ToString()}/{clubId}/";
            GetRequest getRequest = new GetRequest(address);
            SavedRequestHeaders.GetHeaders1(getRequest);

            WebProxy proxy = new WebProxy("127.0.0.1:8888");
            getRequest.Proxy = proxy;

            getRequest.Run(cookieContainer);
            return getRequest.Response;
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
            string patternClub = string.Format(@"href=""\/[{0}]+\/([0-9]+)\/"">([0-9А-Яа-я \-]+)<\/a>", scopesFilter);
            Regex regexClub = new Regex(patternClub);
            Match matchClub = regexClub.Match(htmlCode);
            while (matchClub.Success)
            {
                Console.WriteLine($"{matchClub.Groups[1].Value,-20}{matchClub.Groups[2].Value,20}");
                matchClub = matchClub.NextMatch();
            }
        }

        public void PrintTodayGames(object obj = default)
        {
            Console.Clear();
            string htmlCode = GetHTMLTodayGames();
            string patternClubs = @"title=""([^""]* - [^""]*)"">";
            string patternStartDate = @"""status"".*>([0-9А-Яа-я':.,\- ]+)<.*\/div>";
            string patternScore = @"<div class=""gls"">([0-9\-]+)<\/div>";
            string patternGameId = @"<div id=""gm(\d+)""";
            Regex regexClubs = new Regex(patternClubs);
            Regex regexStartDate = new Regex(patternStartDate);
            Regex regexScore = new Regex(patternScore);
            Regex regexGameId = new Regex(patternGameId);
            Match matchClubs = regexClubs.Match(htmlCode);
            Match matchStartDate = regexStartDate.Match(htmlCode);
            Match matchScore = regexScore.Match(htmlCode);
            Match matchGameId = regexGameId.Match(htmlCode);
            Console.WriteLine(DateTime.Now.ToString("F"));
            Console.WriteLine(new string('-', 25));
            while (matchClubs.Success || matchStartDate.Success || matchScore.Success)
            {
                Console.Write($"{matchStartDate.Groups[1].Value,-20}{matchClubs.Groups[1].Value,50}{matchScore.Groups[1].Value,10}");
                matchScore = matchScore.NextMatch();
                Console.WriteLine($":{matchScore.Groups[1].Value}{matchGameId.Groups[1].Value,20}");
                matchScore = matchScore.NextMatch();
                matchClubs = matchClubs.NextMatch();
                matchStartDate = matchStartDate.NextMatch();
                matchGameId = matchGameId.NextMatch();
            }
        }

        public void PrintClubInfo(int clubId)
        {
            Console.Clear();
            string htmlCode = GetHTMLInfo(clubId,SearchScope.clubs);
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo,indexEndInfo-indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            if (matchTitle.Success)
            {
                Console.WriteLine($"{matchTitle.Groups[1].Value.Trim()}\n{matchTitle.Groups[2].Value.Trim()}");
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);
                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    if (matchKeyInfo.Groups[1].Value == "Стадион")
                    {
                        Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                        matchValueInfo = matchValueInfo.NextMatch();
                        Console.WriteLine($"Город-страна{"",8}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    }
                    else
                        Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

        public void PrintClubInfo(string clubName)
        {
            Console.Clear();
            int clubId = GetClubId(clubName);
            string htmlCode = GetHTMLInfo(clubId,SearchScope.clubs);
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            if (matchTitle.Success)
            {
                Console.WriteLine($"{matchTitle.Groups[1].Value.Trim()}\n{matchTitle.Groups[2].Value.Trim()}");
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
                        Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                        matchValueInfo = matchValueInfo.NextMatch();
                        Console.WriteLine($"Город-страна{"",8}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    }
                    else
                        Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

        public void PrintPlayerInfo(int playerId)
        {
            Console.Clear();
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            if (matchTitle.Success)
            {
                Console.WriteLine($"{matchTitle.Groups[1].Value.Trim()}\n{matchTitle.Groups[2].Value.Trim()}");
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);
                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

        public void PrintPlayerInfo(string playerName)
        {
            Console.Clear();
            int playerId = GetPlayerId(playerName);
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            if (matchTitle.Success)
            {
                Console.WriteLine($"{matchTitle.Groups[1].Value.Trim()}\n{matchTitle.Groups[2].Value.Trim()}");
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);
                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

        public void PrintCoachInfo(int coachId)
        {
            Console.Clear();
            string htmlCode = GetHTMLInfo(coachId, SearchScope.coaches);
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
                    Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

        public void PrintCoachInfo(string coachName)
        {
            Console.Clear();
            int coachId = GetCoachId(coachName);
            string htmlCode = GetHTMLInfo(coachId, SearchScope.coaches);
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
                    Console.WriteLine($"{matchKeyInfo.Groups[1].Value.Trim(),-20}{matchValueInfo.Groups[1].Value.Trim(),-30}");
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
            }
        }

        public void PrintGameInfo(int gameId)
        {
            Console.Clear();
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            string patternClubs = @"var game_[ah]?t_title = '([0-9А-Яа-я \-]+)'";
            string patternScore = @"<div class=""live_game_goal"">\s*<span>([0-9\-]+)<\/span>";
            string patternEventsHome = @"<div class=""event_ht"">\s.*\s*(|<span class=""gray assist"".*\(([0-9А-Яа-я \-\.]+)\)<\/span>)\s.*>([0-9А-Яа-я \-]+)<\/a>.*""event_ht_icon live_([a-z]+)""><\/div>\s.*\s*<div class=""event_min"">([0-9]+)'<\/div>";
            string patternEventsAway = @"<div class=""event_min"">([0-9]+)'<\/div>\s*<div class=""event_at"">\s*<div class=""event_at_icon live_([a-z]+)""><\/div>\s.*>([0-9А-Яа-я \-]+)<\/a><\/span><\/div>\s*(<span class=""gray assist"" title=""Ассистент"">\(([0-9А-Яа-я \-\.]+)\)|)";
            Regex regexClubs = new Regex(patternClubs);
            Regex regexScore = new Regex(patternScore);
            Regex regexEventsHome = new Regex(patternEventsHome);
            Regex regexEventsAway = new Regex(patternEventsAway);
            Match matchClubs = regexClubs.Match(htmlCode);
            Match matchScore = regexScore.Match(htmlCode);
            Match matchEventHome = regexEventsHome.Match(htmlCode);
            Match matchEventAway = regexEventsAway.Match(htmlCode);
            if (matchClubs.Success && matchScore.Success)
            {
                Console.WriteLine($"{matchClubs.Groups[1].Value,-30}{matchScore.Groups[1].Value,-3}:{matchScore.NextMatch().Groups[1].Value,3}{matchClubs.NextMatch().Groups[1].Value,30}");
                while (matchEventHome.Success || matchEventAway.Success)
                {
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            FootballAPI footballAPI = new FootballAPI();
            footballAPI.PrintGameInfo(1563736);

            #region PrintTodayMatches
            //while (true)
            //{
            //    TimerCallback tm = new TimerCallback(footballAPI.PrintTodayMatches);
            //    Timer timer = new Timer(tm, null, 0, 30000);
            //    if (Console.ReadKey(true).Key == ConsoleKey.Enter)
            //    {
            //        timer.Dispose();
            //        return;
            //    }
            //}
            #endregion

            //footballAPI.PrintClubInfo(Console.ReadLine());
        }
    }
}
