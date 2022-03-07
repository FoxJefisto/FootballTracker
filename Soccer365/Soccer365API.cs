using Rest;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using Soccer365.Models;

namespace Soccer365
{
    public class Soccer365Api
    {
        public enum SearchScope
        {
            coaches,
            players,
            clubs,
            games
        }

        public readonly RestClient restClient = new RestClient();

        private string GetHTMLMatches(DateTime date)
        {
            string dateStr = $"{date.Year}-{date.Month}-{date.Day}";

            string address = $"https://soccer365.ru/online/&date={dateStr}";

            return restClient.GetStringAsync(address).Result;
        }
        private string GetHTMLSearch(string value)
        {
            string address = "https://soccer365.ru/?a=search&q=" + HttpUtility.UrlEncode(value);

            return restClient.GetStringAsync(address).Result;
        }
        private string GetHTMLInfo(int clubId, SearchScope scope)
        {
            string address = $"https://soccer365.ru/{scope.ToString()}/{clubId}/";

            return restClient.GetStringAsync(address).Result;
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
        //Сделано!
        private ListOfMatches GetMatches(string htmlCode)
        {
            string patternClubs = @"title=""([^""]+) - ([^""]+)"">";
            string patternMatchStatus = @"""status"".*>([0-9А-Яа-я':.,\- ]+)<.*\/div>";
            string patternScore = @"<div class=""gls"">([0-9\-]+)<\/div>";
            string patternMatchId = @"<div id=""gm(\d+)""";
            string patternDate = @"dt-date=[^>]+>([0-9\.]+)<\/span><\/div>";
            Regex regexClubs = new Regex(patternClubs);
            Regex regexMatchStatus = new Regex(patternMatchStatus);
            Regex regexScore = new Regex(patternScore);
            Regex regexMatchId = new Regex(patternMatchId);
            Regex regexDate = new Regex(patternDate);
            Match matchClubs = regexClubs.Match(htmlCode);
            Match matchMatchStatus = regexMatchStatus.Match(htmlCode);
            Match matchScore = regexScore.Match(htmlCode);
            Match matchMatchId = regexMatchId.Match(htmlCode);
            Match matchDate = regexDate.Match(htmlCode);
            List<FootballMatch> footballMatches = new List<FootballMatch>();
            while (matchClubs.Success || matchMatchStatus.Success || matchScore.Success)
            {
                int matchId = int.Parse(matchMatchId.Groups[1].Value);
                string clubHomeName = matchClubs.Groups[1].Value;
                string clubAwayName = matchClubs.Groups[2].Value;
                FootballClub clubHome = new FootballClub(clubHomeName);
                FootballClub clubAway = new FootballClub(clubAwayName);
                string matchStatus = matchMatchStatus.Groups[1].Value;
                int? clubHomeGoals, clubAwayGoals;
                if (matchScore.Groups[1].Value != "-")
                    clubHomeGoals = int.Parse(matchScore.Groups[1].Value);
                else
                    clubHomeGoals = null;
                matchScore = matchScore.NextMatch();
                if (matchScore.Groups[1].Value != "-")
                    clubAwayGoals = int.Parse(matchScore.Groups[1].Value);
                else
                    clubAwayGoals = null;
                FootballMatch footballMatch = new FootballMatch(matchId, clubHome, clubAway, matchStatus, clubHomeGoals, clubAwayGoals);
                footballMatches.Add(footballMatch);
                matchScore = matchScore.NextMatch();
                matchClubs = matchClubs.NextMatch();
                matchMatchStatus = matchMatchStatus.NextMatch();
                matchMatchId = matchMatchId.NextMatch();
            }
            DateTime date = DateTime.Parse(matchDate.Groups[1].Value);
            ListOfMatches listOfMatches = new ListOfMatches(date, footballMatches);
            return listOfMatches;
        }
        //Сделано!
        public ListOfMatches GetTodayMatches()
        {
            string htmlCode = GetHTMLMatches(DateTime.Now);
            return GetMatches(htmlCode);
        }
        //Сделано!
        public ListOfMatches GetCustomDateMatches(DateTime date)
        {
            string htmlCode = GetHTMLMatches(date);
            return GetMatches(htmlCode);
        }
        //Сделано!
        public void PrintMatches(ListOfMatches listOfMatches, object obj = default)
        {
            Console.Clear();
            Console.WriteLine($"{listOfMatches.Date.ToString("d")}");
            foreach (var match in listOfMatches.footballMatches)
            {
                Console.WriteLine($"{match.MatchStatus,-18}{match.ClubHome.Name,40}{"",6}{match.MatchScore,-10}{match.ClubAway.Name,-40}");
            }
        }

        public void PrintClubInfo(int clubId)
        {
            Console.Clear();
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs);
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
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs);
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
}
