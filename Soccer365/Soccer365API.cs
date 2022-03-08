using Rest;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using Soccer365.Models;
using System.Linq;

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
        //Сделано
        private string GetHTMLMatches(DateTime date)
        {
            string dateStr = $"{date.Year}-{date.Month}-{date.Day}";

            string address = $"https://soccer365.ru/online/&date={dateStr}";

            return restClient.GetStringAsync(address).Result;
        }
        //Сделано
        private string GetHTMLSearch(string value)
        {
            string address = "https://soccer365.ru/?a=search&q=" + HttpUtility.UrlEncode(value);

            return restClient.GetStringAsync(address).Result;
        }
        //Сделано
        private string GetHTMLInfo(string clubId, SearchScope scope)
        {
            string address = $"https://soccer365.ru/{scope.ToString()}/{clubId}/";

            return restClient.GetStringAsync(address).Result;
        }
        private string GetClubId(string club)
        {
            string htmlCode = GetHTMLSearch(club);
            string patternClubId = @"href=""\/clubs\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return matchClubId.Groups[1].Value;
        }

        private string GetCoachId(string coach)
        {
            string htmlCode = GetHTMLSearch(coach);
            string patternClubId = @"href=""\/coaches\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return matchClubId.Groups[1].Value;
        }

        private string GetPlayerId(string player)
        {
            string htmlCode = GetHTMLSearch(player);
            string patternClubId = @"href=""\/players\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return matchClubId.Groups[1].Value;
        }
        //Сделано
        private Dictionary<string, string> Search(string value, params SearchScope[] scope)
        {
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
            Dictionary<string, string> keyValues = new Dictionary<string,string>();
            while (matchClub.Success)
            {
                keyValues.Add(matchClub.Groups[1].Value, matchClub.Groups[2].Value);
                matchClub = matchClub.NextMatch();
            }
            return keyValues;
        }
        //Сделано
        public Dictionary<string, string> GetPlayers(string playerName)
        {
            return Search(playerName, SearchScope.players);
        }
        //Сделано
        public Dictionary<string, string> GetCoaches(string coachName)
        {
            return Search(coachName, SearchScope.coaches);
        }
        //Сделано
        public Dictionary<string, string> GetClubs(string clubName)
        {
            return Search(clubName, SearchScope.clubs);
        }

        //Сделано
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
                string matchId = matchMatchId.Groups[1].Value;
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
        //Сделано
        public ListOfMatches GetTodayMatches()
        {
            string htmlCode = GetHTMLMatches(DateTime.Now);
            return GetMatches(htmlCode);
        }
        //Сделано
        public ListOfMatches GetMatchesByDate(DateTime date)
        {
            string htmlCode = GetHTMLMatches(date);
            return GetMatches(htmlCode);
        }
        //Сделано
        public void PrintMatches(ListOfMatches listOfMatches, object obj = default)
        {
            Console.Clear();
            Console.WriteLine($"{listOfMatches.Date.ToString("d")}");
            foreach (var match in listOfMatches.footballMatches)
            {
                Console.WriteLine($"{match.MatchStatus,-18}{match.ClubHome.Name,40}{"",6}{match.MatchScore,-10}{match.ClubAway.Name,-40}");
            }
        }
        //Сделано
        private FootballClubDetails GetClubInfo(string htmlCode, string clubId)
        {
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            FootballClubDetails club = null;
            string name, englishName, fullName, mainCoach, stadium, city, country, foundationDate;
            name = englishName = fullName = mainCoach = stadium = city = country = foundationDate = null;
            int? rating = null;
            List<Competition> competitions = new List<Competition>();
            if (matchTitle.Success)
            {
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);

                name = matchTitle.Groups[1].Value.Trim();
                englishName = matchTitle.Groups[2].Value.Trim();

                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    string valueInfo = matchValueInfo.Groups[1].Value.Trim();
                    switch (matchKeyInfo.Groups[1].Value.Trim())
                    {
                        case "Полное название":
                            fullName = valueInfo;
                            break;
                        case "Главный тренер":
                            mainCoach = valueInfo;
                            break;
                        case "Стадион":
                            stadium = valueInfo;
                            matchValueInfo = matchValueInfo.NextMatch();
                            valueInfo = matchValueInfo.Groups[1].Value.Trim();
                            city = valueInfo.Split(", ")[0];
                            country = valueInfo.Split(", ")[1];
                            break;
                        case "Год основания":
                            foundationDate = valueInfo;
                            break;
                        case "Рейтинг УЕФА":
                            rating = int.Parse(valueInfo.Split(' ')[0]);
                            break;
                        case "Соревнования":
                            while (matchValueInfo.Success)
                            {
                                Competition competition = new Competition(matchValueInfo.Groups[1].Value.Trim());
                                competitions.Add(competition);
                                matchValueInfo = matchValueInfo.NextMatch();
                            }
                            break;

                    }
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
                club = new FootballClubDetails(clubId, name, englishName, fullName, mainCoach, stadium, city, country, foundationDate, rating, competitions);
            }
            return club;
        }
        //Сделано
        public FootballClubDetails GetClubInfoById(string clubId)
        {
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs);
            return GetClubInfo(htmlCode, clubId);
        }
        //Сделано
        public FootballClubDetails GetClubInfoByName(string name)
        {
            string clubId = GetClubId(name);
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs);
            return GetClubInfo(htmlCode, clubId);
        }
        //Сделано
        public void PrintClubInfo(FootballClubDetails club)
        {
            Console.Clear();
            if (club.Id != null)
                Console.WriteLine($"{"ID:", -21}{club.Id,-30}");
            if (club.Name != null)
                Console.WriteLine($"{"Название:",-21}{club.Name,-30}");
            if (club.NameEnglish != null)
                Console.WriteLine($"{"Английское название:",-21}{club.NameEnglish,-30}");
            if (club.FullName != null)
                Console.WriteLine($"{"Полное название:",-21}{club.FullName,-30}");
            if (club.MainCoach != null)
                Console.WriteLine($"{"Главный тренер:",-21}{club.MainCoach,-30}");
            if (club.Stadium != null)
                Console.WriteLine($"{"Стадион:",-21}{club.Stadium,-30}");
            if (club.City != null)
                Console.WriteLine($"{"Город:",-21}{club.City,-30}");
            if (club.Country != null)
                Console.WriteLine($"{"Страна:",-21}{club.Country,-30}");
            if (club.FoundationDate != null)
                Console.WriteLine($"{"Год основания:",-21}{club.FoundationDate,-30}");
            if (club.Rating != null)
                Console.WriteLine($"{"Рейтинг УЕФА:",-21}{club.Rating,-30}");
            if (club.Competitions.Count != 0)
            {
                Console.WriteLine($"{"Соревнования:",-21}");
                foreach (var competition in club.Competitions)
                    Console.WriteLine($"{competition.Name,-51}");
            }
        }
        //Сделано
        private FootballPlayer GetPlayerInfo(string htmlCode, string playerId)
        {
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            FootballPlayer player = null;
            string firstName, lastName, fullName, citizenship, placeOfBirth,
                club, nationalTeam, position, workingLeg;
            firstName = lastName = fullName = citizenship = placeOfBirth = club = nationalTeam = position = workingLeg = null;
            int? numberInClub, numberInNatTeam, height, weight;
            numberInClub = numberInNatTeam = height = weight = null;
            DateTime? dateOfBirth = null;
            if (matchTitle.Success)
            {
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);

                firstName = matchTitle.Groups[1].Value.Split(' ')[0];
                lastName = string.Join(' ', matchTitle.Groups[1].Value.Split(' ').Skip(1).ToArray());

                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    string valueInfo = matchValueInfo.Groups[1].Value.Trim();
                    string patternHeightWeight = @"([0-9]+)[^0-9]*([0-9]+)";
                    Regex regexHeightWeight = new Regex(patternHeightWeight);
                    Match matchHeightWeight;
                    switch (matchKeyInfo.Groups[1].Value.Trim())
                    {
                        case "Полное имя":
                            fullName = valueInfo;
                            break;
                        case "Клуб":
                            club = valueInfo;
                            break;
                        case "Номер в клубе":
                            numberInClub = int.Parse(valueInfo);
                            break;
                        case "Сборная":
                            nationalTeam = valueInfo;
                            break;
                        case "Номер в сборной":
                            numberInNatTeam = int.Parse(valueInfo);
                            break;
                        case "Дата рождения":
                            dateOfBirth = DateTime.Parse(valueInfo.Split(' ')[0]);
                            break;
                        case "Страна рождения":
                            citizenship = valueInfo;
                            break;
                        case "Город рождения":
                            nationalTeam = valueInfo;
                            break;
                        case "Позиция":
                            position = valueInfo;
                            break;
                        case "Рабочая нога":
                            workingLeg = valueInfo;
                            break;
                        case "Рост/вес":
                            matchHeightWeight = regexHeightWeight.Match(valueInfo);
                            height = int.Parse(matchHeightWeight.Groups[1].Value);
                            weight = int.Parse(matchHeightWeight.Groups[2].Value);
                            break;
                    }
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
                player = new FootballPlayer(firstName, lastName, fullName, dateOfBirth, citizenship, 
                    placeOfBirth, playerId, club, numberInClub, nationalTeam, numberInNatTeam, position, workingLeg, height, weight);
            }
            return player;
        }
        //Сделано
        public FootballPlayer GetPlayerInfoById(string playerId)
        {
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            return GetPlayerInfo(htmlCode, playerId);
        }
        //Сделано
        public FootballPlayer GetPlayerInfoByName(string name)
        {
            string playerId = GetPlayerId(name);
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            return GetPlayerInfo(htmlCode, playerId);
        }
        //Сделано
        public void PrintPlayerInfo(FootballPlayer player)
        {
            Console.Clear();
            if (player.Id != null)
                Console.WriteLine($"{"ID:",-21}{player.Id,-30}");
            if (player.FirstName != null)
                Console.WriteLine($"{"Имя:",-21}{player.FirstName,-30}");
            if (player.LastName != null)
                Console.WriteLine($"{"Фамилия:",-21}{player.LastName,-30}");
            if (player.FullName != null)
                Console.WriteLine($"{"Полное имя:",-21}{player.FullName,-30}");
            if (player.Club != null)
                Console.WriteLine($"{"Клуб:",-21}{player.Club,-30}");
            if (player.NumberInClub != null)
                Console.WriteLine($"{"Номер в клубе:",-21}{player.NumberInClub,-30}");
            if (player.NationalTeam != null)
                Console.WriteLine($"{"Сборная:",-21}{player.NationalTeam,-30}");
            if (player.NumberInNatTeam != null)
                Console.WriteLine($"{"Номер в сборной:",-21}{player.NumberInNatTeam,-30}");
            if (player.Citizenship != null)
                Console.WriteLine($"{"Страна рождения:",-21}{player.Citizenship,-30}");
            if (player.PlaceOfBirth != null)
                Console.WriteLine($"{"Город рождения:",-21}{player.PlaceOfBirth,-30}");
            if (player.Position != null)
                Console.WriteLine($"{"Позиция:",-21}{player.Position,-30}");
            if (player.WorkingLeg != null)
                Console.WriteLine($"{"Рабочая нога:",-21}{player.WorkingLeg,-30}");
            if (player.Height != null)
                Console.WriteLine($"{"Рост:",-21}{player.Height,-30}");
            if (player.Weight != null)
                Console.WriteLine($"{"Вес:",-21}{player.Weight,-30}");
        }
        //Сделано
        private Coach GetCoachInfo(string htmlCode, string coachId)
        {
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>\s*<div class=""profile_en_title"">([0-9A-Za-z \-]+)<\/div>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            Coach coach = null;
            string firstName, lastName, fullName, citizenship, placeOfBirth, club;
            firstName = lastName = fullName = citizenship = placeOfBirth = club = null;
            int? height, weight;
            height = weight = null;
            DateTime? dateOfBirth = null;
            if (matchTitle.Success)
            {
                string patternKeyInfo = @"<td class=""params_key[^""]*"">((?!Ссылки)[^<]+)";
                string patternValueInfo = @">\s*([^<]+)(<\/a><\/span><\/div>|<\/span>|<\/td><\/tr>|<span class=""[^""]+"">)";
                Regex regexKeyInfo = new Regex(patternKeyInfo);
                Regex regexValueInfo = new Regex(patternValueInfo);
                Match matchKeyInfo = regexKeyInfo.Match(htmlCode);
                Match matchValueInfo = regexValueInfo.Match(htmlCode);

                firstName = matchTitle.Groups[1].Value.Split(' ')[0];
                lastName = string.Join(' ', matchTitle.Groups[1].Value.Split(' ').Skip(1).ToArray());

                while (matchKeyInfo.Success || matchValueInfo.Success)
                {
                    string valueInfo = matchValueInfo.Groups[1].Value.Trim();
                    string patternHeightWeight = @"([0-9]+)[^0-9]*([0-9]+)";
                    Regex regexHeightWeight = new Regex(patternHeightWeight);
                    Match matchHeightWeight;
                    switch (matchKeyInfo.Groups[1].Value.Trim())
                    {
                        case "Имя":
                            fullName = valueInfo;
                            break;
                        case "Фамилия":
                            fullName += " " + valueInfo;
                            break;
                        case "Команда":
                            club = valueInfo;
                            break;
                        case "Гражданство":
                            citizenship = valueInfo;
                            break;
                        case "Дата рождения":
                            dateOfBirth = DateTime.Parse(valueInfo.Split(' ')[0]);
                            break;
                        case "Страна рождения":
                            placeOfBirth = valueInfo;
                            break;
                        case "Город рождения":
                            placeOfBirth +=", " + valueInfo;
                            break;
                        case "Рост/вес":
                            matchHeightWeight = regexHeightWeight.Match(valueInfo);
                            height = int.Parse(matchHeightWeight.Groups[1].Value);
                            weight = int.Parse(matchHeightWeight.Groups[2].Value);
                            break;
                    }
                    matchKeyInfo = matchKeyInfo.NextMatch();
                    matchValueInfo = matchValueInfo.NextMatch();
                }
                coach = new Coach(firstName, lastName, fullName, dateOfBirth, citizenship, placeOfBirth, coachId, club, height, weight);
            }
            return coach;
        }
        //Сделано
        public Coach GetCoachInfoById(string coachId)
        {
            string htmlCode = GetHTMLInfo(coachId, SearchScope.coaches);
            return GetCoachInfo(htmlCode, coachId);
        }
        //Сделано
        public Coach GetCoachInfoByName(string name)
        {
            string coachId = GetCoachId(name);
            string htmlCode = GetHTMLInfo(coachId, SearchScope.coaches);
            return GetCoachInfo(htmlCode, coachId);
        }
        //Сделано
        public void PrintCoachInfo(Coach coach)
        {
            Console.Clear();
            if (coach.Id != null)
                Console.WriteLine($"{"ID:",-21}{coach.Id,-30}");
            if (coach.FirstName != null)
                Console.WriteLine($"{"Имя:",-21}{coach.FirstName,-30}");
            if (coach.LastName != null)
                Console.WriteLine($"{"Фамилия:",-21}{coach.LastName,-30}");
            if (coach.FullName != null)
                Console.WriteLine($"{"Полное имя:",-21}{coach.FullName,-30}");
            if (coach.Club != null)
                Console.WriteLine($"{"Команда:",-21}{coach.Club,-30}");
            if (coach.Citizenship != null)
                Console.WriteLine($"{"Гражданство:",-21}{coach.Citizenship,-30}");
            if (coach.PlaceOfBirth != null)
                Console.WriteLine($"{"Место рождения:",-21}{coach.PlaceOfBirth,-30}");
            if (coach.Height != null)
                Console.WriteLine($"{"Рост:",-21}{coach.Height,-30}");
            if (coach.Weight != null)
                Console.WriteLine($"{"Вес:",-21}{coach.Weight,-30}");
        }
        public void PrintGameInfo(string gameId)
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
