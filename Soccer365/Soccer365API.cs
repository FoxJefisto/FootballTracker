using Newtonsoft.Json.Linq;
using Rest;
using Soccer365.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Soccer365
{
    public class Soccer365Api
    {
        private enum SearchScope
        {
            coaches,
            players,
            clubs,
            games,
            competitions
        }

        public enum SortScope
        {
            goals = 1,
            assists,
            matches,
            minutes,
            goalPlusPass,
            penGoals,
            doubleGoals,
            hatTricks,
            yellowCards,
            yellowRedCards,
            redCards,
            fairPlayScore
        }

        public enum SortOrder
        {
            ASC = 0,
            DESC = 1
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
        private string GetHTMLSearchCompetition(string value)
        {
            string address = "https://soccer365.ru/index.php?c=competitions&a=champs_list_data&tp=0&cn_id=0&st=0&ttl=" + HttpUtility.UrlEncode(value);
            return restClient.GetStringAsync(address).Result;
        }
        //Сделано
        private string GetHTMLInfo(string id, SearchScope scope, string data = null)
        {
            string address = $"https://soccer365.ru/{scope}/{id}/";
            if (data != null)
                address += data;

            return restClient.GetStringAsync(address).Result;
        }
        private string GetClubIdByName(string club)
        {
            string htmlCode = GetHTMLSearch(club);
            string patternClubId = @"href=""\/clubs\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return matchClubId.Groups[1].Value;
        }
        public FootballClub GetClubById(string clubId)
        {
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs);
            Match matchTitle = Regex.Match(htmlCode, @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>");
            if (matchTitle.Success)
                return new FootballClub(clubId, matchTitle.Groups[1].Value);
            else
                return null;
        }
        private string GetCoachIdByName(string coach)
        {
            string htmlCode = GetHTMLSearch(coach);
            string patternClubId = @"href =""\/coaches\/([0-9]+)";
            Regex regexClubId = new Regex(patternClubId);
            Match matchClubId = regexClubId.Match(htmlCode);
            return matchClubId.Groups[1].Value;
        }
        public Person GetCoachById(string coachId)
        {
            string htmlCode = GetHTMLInfo(coachId, SearchScope.coaches);
            Match matchName = Regex.Match(htmlCode, @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>");
            if (matchName.Success) {
                string firstName = matchName.Groups[1].Value.Split(' ')[0];
                string lastName = string.Join(" ", matchName.Groups[1].Value.Split(' ').Skip(1).ToArray());
                return new Person(coachId, firstName, lastName);
            }
            else
                return null;
        }
        private string GetPlayerIdByName(string player)
        {
            string htmlCode = GetHTMLSearch(player);
            Match matchClubId = Regex.Match(htmlCode, @"href=""\/players\/([0-9]+)");
            return matchClubId.Groups[1].Value;
        }
        public Person GetPlayerById(string playerId)
        {
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            Match matchName = Regex.Match(htmlCode, @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>");
            if (matchName.Success)
            {
                string firstName = matchName.Groups[1].Value.Split(' ')[0];
                string lastName = string.Join(" ", matchName.Groups[1].Value.Split(' ').Skip(1).ToArray());
                return new Person(playerId, firstName, lastName);
            }
            else
                return null;
        }
        public Competitions GetCompetitionById(string competitionId)
        {
            string htmlCode = GetHTMLInfo(competitionId, SearchScope.competitions);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlCode);
            var path = doc.DocumentNode.SelectSingleNode(".//div[@class='page_main_bk_right']/div[@class='block_body']");
            var name = path.SelectSingleNode(".//h1[@class='profile_info_title red']").InnerText;
            var table = path.SelectNodes(".//td");
            string country = table[0].InnerText != "Страна" ? "" : table[1].InnerText;
            return new Competitions(competitionId, name, country);
        }
        private string GetCompetitionIdByName(string name, string country)
        {
            string htmlCode = GetHTMLSearchCompetition(country);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlCode);
            var comps = doc.DocumentNode.SelectNodes(".//div[@class='season_item']");
            string id = null;
            foreach (var comp in comps)
            {
                Match matchCountry = Regex.Match(comp.InnerHtml, $@"title=""{country}""", RegexOptions.IgnoreCase);
                Match matchName = Regex.Match(comp.InnerHtml, $@">{name}</span></div>", RegexOptions.IgnoreCase);
                if(matchCountry.Success && matchName.Success)
                {
                    id = Regex.Match(comp.InnerHtml, @"competitions/(\d+)").Groups[1].Value;
                    break;
                }
            }
            return id;
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
            string patternClub = string.Format(@"href=""\/[{0}]+\/([0-9]+)\/"">([^<]+)<\/a>", scopesFilter);
            Regex regexClub = new Regex(patternClub);
            Match matchClub = regexClub.Match(htmlCode);
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            while (matchClub.Success)
            {
                keyValues.Add(matchClub.Groups[1].Value, matchClub.Groups[2].Value);
                matchClub = matchClub.NextMatch();
            }
            return keyValues;
        }
        //Сделано
        public List<Person> GetPlayers(string playerName)
        {
            var results = Search(playerName, SearchScope.players);
            List<Person> players = new List<Person>();
            foreach (var player in results)
            {
                string firstName = player.Value.Split(' ')[0];
                string lastName = string.Join(' ', player.Value.Split(' ').Skip(1).ToArray());
                players.Add(new Person(player.Key, firstName, lastName));
            }
            return players;
        }
        //Сделано
        public List<Person> GetCoaches(string coachName)
        {
            var results = Search(coachName, SearchScope.coaches);
            List<Person> coaches = new List<Person>();
            foreach (var coach in results)
            {
                string firstName = coach.Value.Split(' ')[0];
                string lastName = string.Join(' ', coach.Value.Split(' ').Skip(1).ToArray());
                coaches.Add(new Person(coach.Key, firstName, lastName));
            }
            return coaches;
        }
        //Сделано
        public List<FootballClub> GetClubs(string clubName)
        {
            var results = Search(clubName, SearchScope.clubs);
            List<FootballClub> clubs = new List<FootballClub>();
            foreach (var club in results)
            {
                clubs.Add(new FootballClub(club.Key, club.Value));
            }
            return clubs;
        }
        public List<Competitions> GetCompetitionsByCountry(string country)
        {
            string htmlCode = GetHTMLSearchCompetition(country);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlCode);
            var comps = doc.DocumentNode.SelectNodes(".//div[@class='season_item']");
            var competitions = new List<Competitions>();
            foreach (var comp in comps)
            {
                Match matchName = Regex.Match(comp.InnerHtml, @">([^<]+)</span></div>");
                Match matchId = Regex.Match(comp.InnerHtml, @"competitions/(\d+)");
                if(matchName.Success && matchId.Success)
                {
                    string name = matchName.Groups[1].Value;
                    string id = matchId.Groups[1].Value;
                    string season = comp.SelectSingleNode(".//div[@class='info']").InnerText.Trim();
                    competitions.Add(new Competitions(id, name, country, season));
                }
            }
            return competitions;
        }
        public List<Competitions> GetCompetitionsByName(string name)
        {
            string htmlCode = GetHTMLSearchCompetition(name);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlCode);
            var comps = doc.DocumentNode.SelectNodes(".//div[@class='season_item']");
            var competitions = new List<Competitions>();
            foreach (var comp in comps)
            {
                Match matchCountry = Regex.Match(comp.InnerHtml, $@"title=""([^""]+)""");
                Match matchId = Regex.Match(comp.InnerHtml, @"competitions/(\d+)");
                if (matchCountry.Success && matchId.Success)
                {
                    string country = matchCountry.Groups[1].Value;
                    string id = matchId.Groups[1].Value;
                    string season = comp.SelectSingleNode(".//div[@class='info']").InnerText.Trim();
                    competitions.Add(new Competitions(id, name, country, season));
                }
            }
            return competitions;
        }
        public void PrintPerson(Person person)
        {
            Console.WriteLine($"{person.Name}");
        }
        //Сделано
        private ListOfMatches GetMatches(string htmlCode, string compName = "", string compCountry = "", int limitGroup = 100)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlCode);
            var groups = doc.DocumentNode.SelectNodes(".//div[@class='live_comptt_bd']");
            List<FootballMatch> footballMatches = new List<FootballMatch>();
            Match matchDate = Regex.Match(htmlCode, @"dt-date=[^>]+>([0-9\.]+)<\/span><\/div>");
            int iGroup = 0;
            foreach (var group in groups)
            {
                Match matchClubs = Regex.Match(group.InnerHtml, @"title=""([^""]+) - ([^""]+)"">");
                Match matchMatchStatus = Regex.Match(group.InnerHtml, @"""status"".*>([0-9А-Яа-я':.,\- ]+)<.*\/div>");
                Match matchScore = Regex.Match(group.InnerHtml, @"<div class=""gls"">([0-9\-]+)<\/div>");
                Match matchMatchId = Regex.Match(group.InnerHtml, @"<div id=""gm(\d+)""");
                Match matchStage = Regex.Match(group.InnerHtml, @"stage"">([^<]+)");
                Match matchCompetitionId = Regex.Match(group.InnerHtml, @"competitions/([\d]+)");
                var matchName = group.SelectSingleNode(".//div[@class='block_header']/div[@class='img16']/span");
                Competitions competition = null;
                if (matchCompetitionId.Success)
                {
                    string competitionId = matchCompetitionId.Groups[1].Value;
                    competition = GetCompetitionById(competitionId);
                } else if(matchName != null)
                {
                    competition = new Competitions(matchName.InnerText);
                }
                bool isFiltred = !competition.Country.Contains(compCountry, StringComparison.OrdinalIgnoreCase) || !competition.Name.Contains(compName, StringComparison.OrdinalIgnoreCase);
                if (compName != "" && isFiltred)
                    continue;
                while (matchClubs.Success || matchMatchStatus.Success || matchScore.Success || matchStage.Success)
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
                    string stage = matchStage.Groups[1].Value;
                    FootballMatch footballMatch = new FootballMatch(matchId, competition, stage, new Pair<FootballClub>(clubHome, clubAway), matchStatus, new Pair<int?>(clubHomeGoals, clubAwayGoals));
                    footballMatches.Add(footballMatch);
                    matchScore = matchScore.NextMatch();
                    matchClubs = matchClubs.NextMatch();
                    matchMatchStatus = matchMatchStatus.NextMatch();
                    matchMatchId = matchMatchId.NextMatch();
                    matchStage = matchStage.NextMatch();
                }
                iGroup++;
                if (iGroup == limitGroup)
                    break;
            }
            DateTime date = DateTime.Parse(matchDate.Groups[1].Value);
            ListOfMatches listOfMatches = new ListOfMatches(date, footballMatches);
            return listOfMatches;
        }
        //Сделано
        public ListOfMatches GetTodayMatches(string compName, string compCountry = "")
        {
            string htmlCode = GetHTMLMatches(DateTime.Now);
            return GetMatches(htmlCode, compName, compCountry);
        }
        public ListOfMatches GetTodayMatches(int limitGroup = 100)
        {
            string htmlCode = GetHTMLMatches(DateTime.Now);
            return GetMatches(htmlCode, "", "", limitGroup);
        }
        //Сделано
        public ListOfMatches GetMatchesByDate(DateTime date, int limitGroup = 100)
        {
            string htmlCode = GetHTMLMatches(date);
            return GetMatches(htmlCode, "", "", limitGroup);
        }

        public ListOfMatches GetMatchesByDate(DateTime date, string compName, string compCountry = "")
        {
            string htmlCode = GetHTMLMatches(date);
            return GetMatches(htmlCode, compName, compCountry);
        }
        //Сделано
        public void PrintMatches(ListOfMatches listOfMatches)
        {
            Console.WriteLine($"{listOfMatches.Date.ToString("d")}");
            var competitions = from match in listOfMatches.Matches
                               group match by match.Competition;
            foreach(var competition in competitions)
            {
                Console.WriteLine();
                Console.WriteLine($"{competition.Key.Name} {competition.Key.Country}");
                foreach (var match in competition)
                {
                    Console.WriteLine($"{match.Status,-18}{match.Clubs.HomeTeam.Name,40}{"",6}{match.Score,-10}{match.Clubs.AwayTeam.Name,-40}");
                }
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
            List<Competitions> competitions = new List<Competitions>();
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
                                Competitions competition = new Competitions(matchValueInfo.Groups[1].Value.Trim());
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
            string clubId = GetClubIdByName(name);
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs);
            return GetClubInfo(htmlCode, clubId);
        }
        //Сделано
        public void PrintClubInfo(FootballClubDetails club)
        {
            Console.Clear();
            if (club.Id != null)
                Console.WriteLine($"{"ID:",-21}{club.Id,-30}");
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
        private PlayerDetails GetPlayerInfo(string htmlCode, string playerId)
        {
            int indexStartInfo = htmlCode.IndexOf("<div class=\"profile_info new\">");
            int indexEndInfo = htmlCode.IndexOf("</tbody></table>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternTitle = @"<h1 class=""profile_info_title red"">([0-9А-Яа-я \-]+)<\/h1>";
            Regex regexTitle = new Regex(patternTitle);
            Match matchTitle = regexTitle.Match(htmlCode);
            PlayerDetails player = null;
            string firstName, lastName, fullName, citizenship, placeOfBirth,
                position, workingLeg;
            FootballClub club = new FootballClub(), nationalTeam = null;
            firstName = lastName = fullName = citizenship = placeOfBirth = position = workingLeg = null;
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
                            club = new FootballClub(GetClubIdByName(valueInfo), valueInfo);
                            break;
                        case "Номер в клубе":
                            numberInClub = int.Parse(valueInfo);
                            break;
                        case "Сборная":
                            nationalTeam = new FootballClub(GetClubIdByName(valueInfo), valueInfo);
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
                            placeOfBirth = valueInfo;
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
                player = new PlayerDetails(playerId, firstName, lastName, fullName, dateOfBirth, citizenship,
                    placeOfBirth, club, numberInClub, nationalTeam, numberInNatTeam, position, workingLeg, height, weight);
            }
            return player;
        }
        //Сделано
        public PlayerDetails GetPlayerInfoById(string playerId)
        {
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            return GetPlayerInfo(htmlCode, playerId);
        }
        //Сделано
        public PlayerDetails GetPlayerInfoByName(string name)
        {
            string playerId = GetPlayerIdByName(name);
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            return GetPlayerInfo(htmlCode, playerId);
        }
        //Сделано
        public void PrintPlayerInfo(PlayerDetails player)
        {
            Console.Clear();
            if (player.Id != null)
                Console.WriteLine($"{"ID:",-21}{player.Id,-30}");
            if (player.FirstName != null)
                Console.WriteLine($"{"Имя:",-21}{player.FirstName,-30}");
            if (player.LastName != null)
                Console.WriteLine($"{"Фамилия:",-21}{player.LastName,-30}");
            if (player.OriginalName != null)
                Console.WriteLine($"{"Полное имя:",-21}{player.OriginalName,-30}");
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
                            placeOfBirth += ", " + valueInfo;
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
                coach = new Coach(coachId, firstName, lastName, fullName, dateOfBirth, citizenship, placeOfBirth, club, height, weight);
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
            string coachId = GetCoachIdByName(name);
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
            if (coach.OriginalName != null)
                Console.WriteLine($"{"Полное имя:",-21}{coach.OriginalName,-30}");
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
        //Сделано
        public MatchStatistics GetMatchStatistics(string matchId)
        {
            string htmlCode = GetHTMLInfo(matchId, SearchScope.games);
            string patternStatisticsKey = @"<div class=""stats_title"">([a-zA-ZА-Яа-я\-% ]+)";
            string patternStatisticsValue = @"<div class=""stats_inf"">([0-9\.]+)<\/div>";
            Regex regexStatisticsKey = new Regex(patternStatisticsKey);
            Regex regexStatisticsValue = new Regex(patternStatisticsValue);
            Match matchStatisticsKey = regexStatisticsKey.Match(htmlCode);
            Match matchStatisticsValue = regexStatisticsValue.Match(htmlCode);
            MatchStatistics statistics = null;
            Pair<float> xg = null, accPasses = null;
            Pair<int> shots, shotsOnTarget, shotsBlocked, saves, ballPossession, corners, fouls, offsides, yCards, rCards, attacks, attacksDangerous, passes, freeKicks, prowing, crosses, tackles;
            shots = shotsOnTarget = shotsBlocked = saves = ballPossession = corners = fouls = offsides = yCards = rCards = attacks = attacksDangerous = passes = freeKicks = prowing = crosses = tackles = null;
            while (matchStatisticsKey.Success || matchStatisticsValue.Success)
            {
                string valueInfo1 = matchStatisticsValue.Groups[1].Value.Trim();
                matchStatisticsValue = matchStatisticsValue.NextMatch();
                string valueInfo2 = matchStatisticsValue.Groups[1].Value.Trim();
                switch (matchStatisticsKey.Groups[1].Value.Trim())
                {
                    case "xG":
                        xg = new Pair<float>(float.Parse(valueInfo1, CultureInfo.InvariantCulture), float.Parse(valueInfo2, CultureInfo.InvariantCulture));
                        break;
                    case "Удары":
                        shots = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Удары в створ":
                        shotsOnTarget = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Блок-но ударов":
                        shotsBlocked = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Сейвы":
                        saves = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Владение %":
                        ballPossession = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Угловые":
                        corners = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Нарушения":
                        fouls = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Офсайды":
                        offsides = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Желтые карточки":
                        yCards = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Красные карточки":
                        rCards = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Атаки":
                        attacks = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Опасные атаки":
                        attacksDangerous = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Передачи":
                        passes = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Точность передач %":
                        accPasses = new Pair<float>(float.Parse(valueInfo1, CultureInfo.InvariantCulture), float.Parse(valueInfo2, CultureInfo.InvariantCulture));
                        break;
                    case "Штрафные удары":
                        freeKicks = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Вбрасывания":
                        prowing = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Навесы":
                        crosses = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;
                    case "Отборы":
                        tackles = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
                        break;

                }
                matchStatisticsKey = matchStatisticsKey.NextMatch();
                matchStatisticsValue = matchStatisticsValue.NextMatch();
            }
            MatchStatisticsStruct st = new MatchStatisticsStruct()
            {
                xg = xg,
                shots = shots,
                shotsOnTarget = shotsOnTarget,
                shotsBlocked = shotsBlocked,
                saves = saves,
                ballPossession = ballPossession,
                corners = corners,
                fouls = fouls,
                offsides = offsides,
                yCards = yCards,
                rCards = rCards,
                attacks = attacks,
                attacksDangerous = attacksDangerous,
                passes = passes,
                accPasses = accPasses,
                freeKicks = freeKicks,
                prowing = prowing,
                crosses = crosses,
                tackles = tackles
            };

            statistics = new MatchStatistics(matchId, st);
            return statistics;
        }
        //Сделано
        public void PrintMatchStatistics(MatchStatistics st)
        {
            if (st.Xg != null)
                Console.WriteLine($"{"xG:",-21}{st.Xg.OutPair()}");
            if (st.Shots != null)
                Console.WriteLine($"{"Удары:",-21}{st.Shots.OutPair()}");
            if (st.ShotsOnTarget != null)
                Console.WriteLine($"{"Удары в створ:",-21}{st.ShotsOnTarget.OutPair()}");
            if (st.ShotsBlocked != null)
                Console.WriteLine($"{"Блок-но ударов:",-21}{st.ShotsBlocked.OutPair()}");
            if (st.Saves != null)
                Console.WriteLine($"{"Сейвы:",-21}{st.Saves.OutPair()}");
            if (st.BallPossession != null)
                Console.WriteLine($"{"Владение мячом %:",-21}{st.BallPossession.OutPair()}");
            if (st.Corners != null)
                Console.WriteLine($"{"Угловые:",-21}{st.Corners.OutPair()}");
            if (st.Fouls != null)
                Console.WriteLine($"{"Нарушения:",-21}{st.Fouls.OutPair()}");
            if (st.Offsides != null)
                Console.WriteLine($"{"Оффсайды:",-21}{st.Offsides.OutPair()}");
            if (st.YCards != null)
                Console.WriteLine($"{"Желтые карточки:",-21}{st.YCards.OutPair()}");
            if (st.RCards != null)
                Console.WriteLine($"{"Красные карточки:",-21}{st.RCards.OutPair()}");
            if (st.Attacks != null)
                Console.WriteLine($"{"Атаки:",-21}{st.Attacks.OutPair()}");
            if (st.AttacksDangerous != null)
                Console.WriteLine($"{"Опасные атаки:",-21}{st.AttacksDangerous.OutPair()}");
            if (st.Passes != null)
                Console.WriteLine($"{"Передачи:",-21}{st.Passes.OutPair()}");
            if (st.AccPasses != null)
                Console.WriteLine($"{"Точность передач:",-21}{st.AccPasses.OutPair()}");
            if (st.FreeKicks != null)
                Console.WriteLine($"{"Штрафные удары:",-21}{st.FreeKicks.OutPair()}");
            if (st.Prowing != null)
                Console.WriteLine($"{"Вбрасывания:",-21}{st.Prowing.OutPair()}");
            if (st.Crosses != null)
                Console.WriteLine($"{"Навесы:",-21}{st.Crosses.OutPair()}");
        }
        //Сделано
        public MatchMainEvents GetMatchMainEvents(string gameId)
        {
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            string patternEventsHome = @"<div class=""event_ht"">\s.*\s*(|<span class=""gray assist"".*\(([0-9А-Яа-я \-\.]+)\)<\/span>)\s.*\/([0-9]+)\/"">([0-9А-Яа-я \-]+)<\/a>.*""event_ht_icon live_([a-z]+)""><\/div>\s.*\s*<div class=""event_min"">([0-9]+)'<\/div>";
            string patternEventsAway = @"<div class=""event_min"">([0-9]+)'<\/div>\s*<div class=""event_at"">\s*<div class=""event_at_icon live_([a-z]+)""><\/div>\s.*\/([0-9]+)\/"">([0-9А-Яа-я \-]+)<\/a><\/span><\/div>\s*(<span class=""gray assist"" title=""Ассистент"">\(([0-9А-Яа-я \-\.]+)\)|)";
            Regex regexEventsHome = new Regex(patternEventsHome);
            Regex regexEventsAway = new Regex(patternEventsAway);
            Match matchEventHome = regexEventsHome.Match(htmlCode);
            Match matchEventAway = regexEventsAway.Match(htmlCode);
            List<MatchMainEvent> eventsHome = new List<MatchMainEvent>();
            List<MatchMainEvent> eventsAway = new List<MatchMainEvent>();
            while (matchEventHome.Success)
            {
                int minute = int.Parse(matchEventHome.Groups[6].Value);
                string eventName = matchEventHome.Groups[5].Value;
                string firstNameMain = matchEventHome.Groups[4].Value.Split(' ')[0];
                string lastNameMain = string.Join(' ', matchEventHome.Groups[4].Value.Split(' ').Skip(1).ToArray());
                string id = matchEventHome.Groups[3].Value;
                Person mainAuthor = new Person(id, firstNameMain, lastNameMain);
                Person secondAuthor = null;
                if (matchEventHome.Groups[2].Value != "")
                {
                    string firstNameSecond = matchEventHome.Groups[2].Value.Split(' ')[0];
                    string lastNameSecond = string.Join(' ', matchEventHome.Groups[2].Value.Split(' ').Skip(1).ToArray());
                    secondAuthor = new Person(firstNameSecond, lastNameSecond);
                }
                MatchMainEvent matchMainEvent = new MatchMainEvent(TeamType.Home, minute, eventName, mainAuthor, secondAuthor);
                eventsHome.Add(matchMainEvent);
                matchEventHome = matchEventHome.NextMatch();
            }
            while (matchEventAway.Success)
            {
                int minute = int.Parse(matchEventAway.Groups[1].Value);
                string eventName = matchEventAway.Groups[2].Value;
                string id = matchEventAway.Groups[3].Value;
                string firstNameMain = matchEventAway.Groups[4].Value.Split(' ')[0];
                string lastNameMain = string.Join(' ', matchEventAway.Groups[4].Value.Split(' ').Skip(1).ToArray());
                Person mainAuthor = new Person(id, firstNameMain, lastNameMain);
                Person secondAuthor = null;
                if (matchEventHome.Groups[6].Value != "")
                {
                    string firstNameSecond = matchEventAway.Groups[6].Value.Split(' ')[0];
                    string lastNameSecond = string.Join(' ', matchEventAway.Groups[6].Value.Split(' ').Skip(1).ToArray());
                    secondAuthor = new Person(firstNameSecond, lastNameSecond);
                }
                MatchMainEvent matchMainEvent = new MatchMainEvent(TeamType.Away, minute, eventName, mainAuthor, secondAuthor);
                eventsAway.Add(matchMainEvent);
                matchEventAway = matchEventAway.NextMatch();
            }
            MatchMainEvents mainEvents = new MatchMainEvents(gameId, eventsHome, eventsAway);
            return mainEvents;
        }
        //Сделано
        public void PrintMatchMainEvents(MatchMainEvents mainEvents)
        {
            foreach (var evnt in mainEvents.Events)
            {
                string str = "";
                if (evnt.Team == TeamType.Home)
                {
                    if (evnt.SecondAuthor != null)
                        str += $"({evnt.SecondAuthor.FirstName} {evnt.SecondAuthor.LastName}) ";
                    str += $"{evnt.MainAuthor.FirstName + ' ' + evnt.MainAuthor.LastName} ";
                    Console.WriteLine($"{str,-29} {evnt.Name,-16} {evnt.Minute,2}");
                }
                if (evnt.Team == TeamType.Away)
                {
                    str += $" {evnt.MainAuthor.FirstName + ' ' + evnt.MainAuthor.LastName}";
                    if (evnt.SecondAuthor != null)
                        str += $" ({evnt.SecondAuthor.FirstName} {evnt.SecondAuthor.LastName})";
                    Console.WriteLine($"{"",47}{evnt.Minute,-7} {evnt.Name,-16} {str,-30}");
                }
            }
        }
        //Сделано
        private Pair<List<MatchPlayer>> GetMatchStartSquad(string htmlCode)
        {
            int indexStartInfo = htmlCode.IndexOf("tm-lineup");
            if (indexStartInfo == -1)
                return new Pair<List<MatchPlayer>>(new List<MatchPlayer>(), new List<MatchPlayer>());
            int indexEndInfo = htmlCode.IndexOf("tm-subst", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternSquad = @"class=""сomposit_player"">\s*<a title=""([^""]+)""\s*href=""\/players\/([0-9]+)\/";
            string patternNumber = @"<span class=""сomposit_num"">([0-9]+)<";
            Regex regexSquad = new Regex(patternSquad);
            Regex regexNumber = new Regex(patternNumber);
            Match matchSquad = regexSquad.Match(htmlCode);
            Match matchNumber = regexNumber.Match(htmlCode);
            int k = 0;
            const int MAX_START_PLAYERS = 11;
            List<MatchPlayer> squadHome = new List<MatchPlayer>();
            while ((matchSquad.Success || matchNumber.Success) && k < MAX_START_PLAYERS)
            {
                int number = int.Parse(matchNumber.Groups[1].Value);
                string id = matchSquad.Groups[2].Value;
                string firstNameMain = matchSquad.Groups[1].Value.Split(' ')[0];
                string lastNameMain = string.Join(' ', matchSquad.Groups[1].Value.Split(' ').Skip(1).ToArray());
                Person player = new Person(id, firstNameMain, lastNameMain);
                squadHome.Add(new MatchPlayer(number, player));
                matchNumber = matchNumber.NextMatch();
                matchSquad = matchSquad.NextMatch();
                k++;
            }
            k = 0;
            List<MatchPlayer> squadAway = new List<MatchPlayer>();
            while ((matchSquad.Success || matchNumber.Success) && k < MAX_START_PLAYERS)
            {
                int number = int.Parse(matchNumber.Groups[1].Value);
                string id = matchSquad.Groups[2].Value;
                string firstNameMain = matchSquad.Groups[1].Value.Split(' ')[0];
                string lastNameMain = string.Join(' ', matchSquad.Groups[1].Value.Split(' ').Skip(1).ToArray());
                Person player = new Person(id, firstNameMain, lastNameMain);
                squadAway.Add(new MatchPlayer(number, player));
                matchNumber = matchNumber.NextMatch();
                matchSquad = matchSquad.NextMatch();
                k++;
            }
            var startSquad = new Pair<List<MatchPlayer>>(squadHome, squadAway);
            return startSquad;
        }
        //Сделано
        private Pair<List<MatchPlayer>> GetMatchReservePlayers(string htmlCode)
        {
            int indexStartInfo = htmlCode.IndexOf("tm-subst");
            if (indexStartInfo == -1)
                return new Pair<List<MatchPlayer>>(new List<MatchPlayer>(), new List<MatchPlayer>());
            int indexEndInfo = htmlCode.IndexOf("<div id=\"tm-players-position-view\"", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);

            indexStartInfo = htmlCode.IndexOf("class=\"сomposit_block\"");
            indexEndInfo = htmlCode.IndexOf("<div class=\"сomposit_block\"", indexStartInfo);
            string htmlCodeHome = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);

            indexStartInfo = htmlCode.IndexOf("class=\"сomposit_block\"", indexEndInfo);
            indexEndInfo = htmlCode.IndexOf("<div class=\"cl_both\"", indexStartInfo);
            string htmlCodeAway = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);

            indexStartInfo = htmlCodeHome.IndexOf("<tbody>");
            indexEndInfo = htmlCodeHome.IndexOf("</tbody>", indexStartInfo);
            htmlCodeHome = htmlCodeHome.Substring(indexStartInfo, indexEndInfo - indexStartInfo);

            indexStartInfo = htmlCodeAway.IndexOf("<tbody>");
            indexEndInfo = htmlCodeAway.IndexOf("</tbody>", indexStartInfo);
            htmlCodeAway = htmlCodeAway.Substring(indexStartInfo, indexEndInfo - indexStartInfo);

            string patternSquad = @"class=""сomposit_player"">\s*<a title=""([^""]+)""\s*href=""\/players\/([0-9]+)\/";
            string patternNumber = @"<span class=""сomposit_num"">([0-9]+)<";
            Regex regexSquad = new Regex(patternSquad);
            Regex regexNumber = new Regex(patternNumber);
            Match matchSquad = regexSquad.Match(htmlCodeHome);
            Match matchNumber = regexNumber.Match(htmlCodeHome);
            List<MatchPlayer> squadHome = new List<MatchPlayer>();
            while (matchSquad.Success || matchNumber.Success)
            {
                int number = int.Parse(matchNumber.Groups[1].Value);
                string id = matchSquad.Groups[2].Value;
                string firstNameMain = matchSquad.Groups[1].Value.Split(' ')[0];
                string lastNameMain = string.Join(' ', matchSquad.Groups[1].Value.Split(' ').Skip(1).ToArray());
                Person player = new Person(id, firstNameMain, lastNameMain);
                squadHome.Add(new MatchPlayer(number, player));
                matchNumber = matchNumber.NextMatch();
                matchSquad = matchSquad.NextMatch();
            }
            List<MatchPlayer> squadAway = new List<MatchPlayer>();
            matchSquad = regexSquad.Match(htmlCodeAway);
            matchNumber = regexNumber.Match(htmlCodeAway);
            while ((matchSquad.Success || matchNumber.Success))
            {
                int number = int.Parse(matchNumber.Groups[1].Value);
                string id = matchSquad.Groups[2].Value;
                string firstNameMain = matchSquad.Groups[1].Value.Split(' ')[0];
                string lastNameMain = string.Join(' ', matchSquad.Groups[1].Value.Split(' ').Skip(1).ToArray());
                Person player = new Person(id, firstNameMain, lastNameMain);
                squadAway.Add(new MatchPlayer(number, player));
                matchNumber = matchNumber.NextMatch();
                matchSquad = matchSquad.NextMatch();
            }
            var reservePlayers = new Pair<List<MatchPlayer>>(squadHome, squadAway);
            return reservePlayers;
        }
        //Сделано
        public MatchSquads GetMatchSquads(string gameId)
        {
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            var startSquad = GetMatchStartSquad(htmlCode);
            var reservePlayers = GetMatchReservePlayers(htmlCode);
            var squads = new MatchSquads(startSquad, reservePlayers);
            return squads;
        }
        //Сделано
        public Pair<Person> GetMatchCoaches(string gameId)
        {
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            int indexStartInfo = htmlCode.IndexOf("Главный тренер");
            if (indexStartInfo == -1)
                return null;
            int indexEndInfo = htmlCode.IndexOf("<table><tbody>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternCoaches = @"Главный тренер.*<span>(|.*\/([^\/]+)\/"">)([^<]+)<";
            Regex regexCoaches = new Regex(patternCoaches);
            Match matchCoaches = regexCoaches.Match(htmlCode);
            Pair<Person> coaches = null;
            if (matchCoaches.Success)
            {
                string id = null;
                if (matchCoaches.Groups[2].Success)
                    id = matchCoaches.Groups[2].Value;
                string firstName = matchCoaches.Groups[3].Value.Split(' ')[0];
                string lastName = string.Join(' ', matchCoaches.Groups[3].Value.Split(' ').Skip(1).ToArray());
                Person coachHome = new Person(id, firstName, lastName);
                matchCoaches = matchCoaches.NextMatch();
                id = null;
                if (matchCoaches.Groups[2].Success)
                    id = matchCoaches.Groups[2].Value;
                firstName = matchCoaches.Groups[3].Value.Split(' ')[0];
                lastName = string.Join(' ', matchCoaches.Groups[3].Value.Split(' ').Skip(1).ToArray());
                Person coachAway = new Person(id, firstName, lastName);
                coaches = new Pair<Person>(coachHome, coachAway);
            }
            return coaches;
        }
        //Сделано
        public List<Person> GetMatchReferee(string gameId)
        {
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            int indexStartInfo = htmlCode.IndexOf("Арбитры");
            if (indexStartInfo == -1)
                return new List<Person>();
            htmlCode = htmlCode.Substring(indexStartInfo);
            string patternReferee = @"\/([^\/]+)\/"">([^<]+)<\/a><\/span>";
            Regex regexReferee = new Regex(patternReferee);
            Match matchReferee = regexReferee.Match(htmlCode);
            List<Person> referee = new List<Person>();
            while (matchReferee.Success)
            {
                string id = matchReferee.Groups[1].Value;
                string firstName = matchReferee.Groups[2].Value.Split(' ')[0];
                string lastName = string.Join(' ', matchReferee.Groups[2].Value.Split(' ').Skip(1).ToArray());
                referee.Add(new Person(id, firstName, lastName));
                matchReferee = matchReferee.NextMatch();
            }
            return referee;
        }
        //Сделано
        public Stadiums GetMatchStadium(string gameId)
        {
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            int indexStartInfo = htmlCode.IndexOf("Стадион");
            int indexEndInfo = htmlCode.IndexOf("<div class=\"preview_item\">", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            string patternStadium = @"([^\/]+)\/"">([^<]+)<\/a><\/span><\/div>[^>]*>([^,]+), ([^<]+)";
            string patternWeather = @">([0-9\+\-]+)°C.*>([^<]+)<\/span>";
            Regex regexStadium = new Regex(patternStadium);
            Regex regexWeather = new Regex(patternWeather);
            Match matchStadium = regexStadium.Match(htmlCode);
            Match matchWeather = regexWeather.Match(htmlCode);
            Stadiums stadium = null;
            if (matchStadium.Success || matchWeather.Success)
            {
                string id = matchStadium.Groups[1].Value;
                string name = matchStadium.Groups[2].Value;
                string city = matchStadium.Groups[3].Value;
                string country = matchStadium.Groups[4].Value;
                string temp = matchWeather.Groups[1].Value;
                string weather = matchWeather.Groups[2].Value;
                stadium = new Stadiums(id, name, city, country, temp, weather);
            }
            return stadium;
        }
        //Сделано
        public int? GetMatchAttendance(string gameId)
        {
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            string patternAttendance = @"Зрителей:<\/span>([^<]+)<";
            Regex regexAttendance = new Regex(patternAttendance);
            Match matchAttendance = regexAttendance.Match(htmlCode);
            int? attendance = null;
            if (matchAttendance.Success)
            {
                attendance = int.Parse(matchAttendance.Groups[1].Value.Replace(",", ""));
            }
            return attendance;
        }
        //Сделано
        public FootballMatch GetMatchById(string gameId)
        {
            string htmlCode = GetHTMLInfo(gameId, SearchScope.games);
            Match matchClubId = Regex.Match(htmlCode, @"game_[h|a]t_id = ([^;]+)");
            Match matchClubName = Regex.Match(htmlCode, @"game_[h|a]t_title = '([^']+)'");
            Match matchStatus = Regex.Match(htmlCode, @".*, ([^<]+)<\/h2>");
            Match matchScore = Regex.Match(htmlCode, @"live_game_goal"">\s*<span>([^<]+)");
            Match matchCompetiton = Regex.Match(htmlCode, @"\/competitions\/([^\/]+)\/"">([^<]+)<\/a>");
            Match matchStage = Regex.Match(htmlCode, @"<h2>([^<]+)");
            FootballMatch footballMatch = null;
            if (matchClubId.Success && matchClubName.Success && matchStatus.Success && matchScore.Success && matchCompetiton.Success && matchStage.Success)
            {
                string clubHomeId = matchClubId.Groups[1].Value;
                matchClubId = matchClubId.NextMatch();
                string clubAwayId = matchClubId.Groups[1].Value;
                string clubHomeName = matchClubName.Groups[1].Value;
                matchClubName = matchClubName.NextMatch();
                string clubAwayName = matchClubName.Groups[1].Value;
                FootballClub clubHome = new FootballClub(clubHomeId, clubHomeName);
                FootballClub clubAway = new FootballClub(clubAwayId, clubAwayName);
                string status = null;
                if (!Regex.IsMatch(matchStatus.Groups[1].Value, @"[0-9]+\.[0-9]+\.[0-9]+ [0-9]+:[0-9]+"))
                    matchStatus = Regex.Match(htmlCode, @"""live_game_status""[^>]*>\s*<b>([^<]+)<\/b>");
                status = matchStatus.Groups[1].Value;
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
                Competitions competition = GetCompetitionById(matchCompetiton.Groups[1].Value);
                var labels = matchStage.Groups[1].Value.Replace(competition.Name + ", ", "").Split(", ");
                string stage = string.Join(", ",labels.Take(labels.Length - 1));
                footballMatch = new FootballMatch(gameId, competition, stage, new Pair<FootballClub>(clubHome, clubAway), status, new Pair<int?>(clubHomeGoals, clubAwayGoals));
            }
            return footballMatch;
        }
        //Сделано
        public FootballMatchDetails GetMatchAllInfo(string gameId)
        {
            var match = GetMatchById(gameId);
            var events = GetMatchMainEvents(gameId);
            var squads = GetMatchSquads(gameId);
            var coaches = GetMatchCoaches(gameId);
            var statistics = GetMatchStatistics(gameId);
            var stadium = GetMatchStadium(gameId);
            var attendance = GetMatchAttendance(gameId);
            var refereeTeam = GetMatchReferee(gameId);
            return new FootballMatchDetails(match, events, match.Competition, match.Stage, squads, coaches, statistics, stadium, attendance, refereeTeam);
        }
        //Сделано
        public void PrintMatchSquads(MatchSquads squads)
        {
            Console.WriteLine($"{"Основной состав:",55}");
            for (int i = 0; i < squads.StartSquad.HomeTeam.Count && i < squads.StartSquad.AwayTeam.Count; i++)
            {
                var playerHome = squads.StartSquad.HomeTeam[i];
                var playerAway = squads.StartSquad.AwayTeam[i];
                Console.WriteLine($"{playerHome.Number + " " + playerHome.Player.Name,40} " + $"{"",14}" +
                    $"{playerAway.Number + " " + playerAway.Player.Name,-40}");
            }
            Console.WriteLine($"{"Запасные игроки:",55}");
            for (int i = 0; i < squads.ReservePlayers.HomeTeam.Count && i < squads.ReservePlayers.AwayTeam.Count; i++)
            {
                var playerHome = squads.ReservePlayers.HomeTeam[i];
                var playerAway = squads.ReservePlayers.AwayTeam[i];
                Console.WriteLine($"{playerHome.Number + " " + playerHome.Player.Name,40} " + $"{"",14}" +
                    $"{playerAway.Number + " " + playerAway.Player.Name,-40}");
            }
        }
        //Сделано
        public void PrintMatchCoaches(Pair<Person> coaches)
        {
            if (coaches != null)
                Console.WriteLine($"{coaches.HomeTeam.Name,40} " + $"{"",14}" +
                    $"{coaches.AwayTeam.Name,-40}");
        }
        //Сделано
        public void PrintMatchReferee(List<Person> referee)
        {
            foreach (var person in referee)
            {
                Console.WriteLine($"{person.Name,-33}");
            }
        }
        //Сделано
        public void PrintMatchStadium(Stadiums stadium)
        {
            if (stadium.Name != null)
                Console.WriteLine($"{"Название:",-21}{stadium.Name,-30}");
            if (stadium.City != null)
                Console.WriteLine($"{"Город:",-21}{stadium.City,-30}");
            if (stadium.Country != null)
                Console.WriteLine($"{"Страна:",-21}{stadium.Country,-30}");
            if (stadium.Temp != null)
                Console.WriteLine($"{"Температура:",-21}{stadium.Temp,-30}");
            if (stadium.Weather != null)
                Console.WriteLine($"{"Погода:",-21}{stadium.Weather,-30}");
        }
        //Сделано
        public void PrintMatchAttendance(int? attendance)
        {
            if (attendance != null)
                Console.WriteLine($"{"Посещаемость:",-21}{attendance,-30}");
        }
        //Сделано
        public void PrintMatch(FootballMatch match)
        {
            if (match.Competition.Name != null && match.Stage != null)
                Console.WriteLine($"{"",37}{match.Competition.Name}, {match.Stage}");
            if (match.Status != null)
                Console.WriteLine($"{"",44}{match.Status}");
            if (match.Clubs != null && match.Score != null)
                Console.WriteLine($"{match.Clubs.HomeTeam.Name,40}{"",5}{match.Score,-10}{match.Clubs.AwayTeam.Name,-40}");
        }
        //Сделано
        public void PrintMatchAllInfo(FootballMatchDetails match)
        {
            PrintMatch(match);
            PrintMatchMainEvents(match.MainEvents);
            Console.WriteLine();
            PrintMatchSquads(match.Squads);
            PrintMatchCoaches(match.Coaches);
            Console.WriteLine("Статистика:");
            PrintMatchStatistics(match.Statistics);
            Console.WriteLine();
            PrintMatchStadium(match.Stadium);
            PrintMatchAttendance(match.Attendance);
            Console.WriteLine();
            Console.WriteLine("Судейская бригада:");
            PrintMatchReferee(match.RefereeTeam);
        }
        //Сделано
        public FootballMatch GetMatchByDateName(DateTime date, string name)
        {
            var matches = GetMatchesByDate(date);
            var match = matches.Matches.Find((x) => { return x.Clubs.HomeTeam.Name.Contains(name) || x.Clubs.AwayTeam.Name.Contains(name); });
            return match;
        }

        public PlayerStatistics GetPlayerStatisticsById(string playerId)
        {
            string htmlCode = GetHTMLInfo(playerId, SearchScope.players);
            int indexStartInfo = htmlCode.IndexOf("pl_stat_block");
            if (indexStartInfo == -1)
                return new PlayerStatistics(playerId, new List<RowPlayerStatistics>());
            htmlCode = htmlCode.Substring(indexStartInfo);
            Match matchCompetition = Regex.Match(htmlCode, @"competitions\/([^\/]+)\/(|([0-9\-]+)\/)"">.*>\s*([А-Яа-я\-\ 0-9\/]+)");
            Match matchClubs = Regex.Match(htmlCode, @"clubs\/([^\/]+)\/"">([^<]+)");
            Match matchStats = Regex.Match(htmlCode, @"<td class[^>]+>([^<]+)<\/td>");
            List<RowPlayerStatistics> stats = new List<RowPlayerStatistics>();
            while (matchCompetition.Success && matchClubs.Success)
            {
                FootballClub club = new FootballClub(matchClubs.Groups[1].Value, matchClubs.Groups[2].Value);
                string season = null;
                if (matchCompetition.Groups[3].Success)
                    season = matchCompetition.Groups[3].Value;
                Competitions competition = GetCompetitionById(matchCompetition.Groups[1].Value);
                competition.Season = season;
                int[] arr = new int[10];
                int i = 0;
                while (matchStats.Success && i != 10)
                {
                    if (!int.TryParse(matchStats.Groups[1].Value, out arr[i]))
                        arr[i] = 0;
                    i++;
                    matchStats = matchStats.NextMatch();
                }
                RowPlayerStatistics row = new RowPlayerStatistics(club, competition, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7], arr[8], arr[9]);
                stats.Add(row);
                matchCompetition = matchCompetition.NextMatch();
                matchClubs = matchClubs.NextMatch();
            }
            PlayerStatistics playerStatistics = new PlayerStatistics(playerId, stats);
            return playerStatistics;
        }
        public PlayerStatistics GetPlayerStatisticsByName(string playerName)
        {
            string playerId = GetPlayerIdByName(playerName);
            return GetPlayerStatisticsById(playerId);
        }
        public void PrintPlayerStatistics(PlayerStatistics stats)
        {
            Console.WriteLine($"{"Команда",-30} {"Соревнование",-40} {"Игр",3} {"Гол",3} {"Пен",3} {"Пер",3} {"Мин",4} {"ВСт",3} {"ВНз",3} {"ЖК",3} {"ЖКК",3} {"КК",3}");
            foreach (var stat in stats.Statistics)
            {
                Console.WriteLine($"{stat.Club.Name,-30} {stat.Competition.Name,-40} {stat.Matches,3} {stat.Goals,3} {stat.PenGoals,3} {stat.Assists,3} {stat.Minutes,4} {stat.InStartMatches,3} " +
                    $"{stat.InSubsMatches,3} {stat.YellowCards,3} {stat.YellowRedCards,3} {stat.RedCards,3}");
            }
        }
        public Squad GetClubSquadById(string clubId)
        {
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs, "&tab=squads");
            int indexStartInfo = htmlCode.IndexOf("<tbody>");
            if (indexStartInfo == -1)
                return new Squad(clubId, new List<Player>());
            int indexEndInfo = htmlCode.IndexOf("</tbody>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            List<Player> players = new List<Player>();
            FootballClub club = GetClubById(clubId);
            indexStartInfo = 0;
            while (htmlCode.IndexOf("<tr>", indexStartInfo) != -1)
            {
                indexStartInfo = htmlCode.IndexOf("<tr>", indexStartInfo);
                indexEndInfo = htmlCode.IndexOf("</tr>", indexStartInfo);
                string htmlCodePlayer = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
                Match matchPlayerId = Regex.Match(htmlCodePlayer, @"href=""\/players\/([0-9]+)");
                Match matchPlayerName = Regex.Match(htmlCodePlayer, @"<span>([^<]+)<\/span>");
                Match matchPlayerPosition = Regex.Match(htmlCodePlayer, @"<br\/>([^<]+)<\/div>");
                Match matchAgeBirth = Regex.Match(htmlCodePlayer, @"([0-9]+) [а-я]+<br><span class=""min_gray"">([^<]+)<\/span>");
                Match matchNumber = Regex.Match(htmlCodePlayer, @"tb_center""><span class=""num_circle"">([0-9]+)<\/span><\/td>");
                Match matchWorkingLeg = Regex.Match(htmlCodePlayer, @"<td>([^<]+)<");
                DateTime? dateOfBirth = null;
                string playerId, firstName, lastName, position, workingLeg;
                int? age, number, height, weight;
                playerId = firstName = lastName = position = workingLeg = null;
                age = number = height = weight = null;
                playerId = matchPlayerId.Groups[1].Value;
                firstName = matchPlayerName.Groups[1].Value.Split(' ')[0];
                lastName = string.Join(' ', matchPlayerName.Groups[1].Value.Split(' ').Skip(1).ToArray());
                position = matchPlayerPosition.Groups[1].Value;
                if (matchAgeBirth.Success)
                {
                    age = int.Parse(matchAgeBirth.Groups[1].Value);
                    dateOfBirth = DateTime.Parse(matchAgeBirth.Groups[2].Value);
                }
                if (matchWorkingLeg.Success)
                    workingLeg = matchWorkingLeg.Groups[1].Value;
                if (matchNumber.Success)
                    number = int.Parse(matchNumber.Groups[1].Value);
                string htmlHeight = htmlCodePlayer.Substring(300);
                Match matchHeightWeight = Regex.Match(htmlHeight, @"<td class=""tb_center"">([^<]*)<\/td>");
                if (matchHeightWeight.Groups[1].Value != "")
                    height = int.Parse(matchHeightWeight.Groups[1].Value.Replace(".", ""));
                matchHeightWeight = matchHeightWeight.NextMatch();
                if (matchHeightWeight.Groups[1].Value != "")
                    weight = int.Parse(matchHeightWeight.Groups[1].Value);
                Player player = new Player(club, playerId, firstName, lastName, number, position, age, dateOfBirth, workingLeg, height, weight);
                players.Add(player);
                matchPlayerId = matchPlayerId.NextMatch();
                matchPlayerName = matchPlayerName.NextMatch();
                matchPlayerPosition = matchPlayerPosition.NextMatch();
                matchAgeBirth = matchAgeBirth.NextMatch();
                matchNumber = matchNumber.NextMatch();
                matchHeightWeight = matchHeightWeight.NextMatch();
                matchWorkingLeg = matchWorkingLeg.NextMatch();
                indexStartInfo = indexEndInfo + 3;
            }
            return new Squad(clubId, players);
        }
        public Squad GetClubSquadByName(string clubName)
        {
            string clubId = GetClubIdByName(clubName);
            return GetClubSquadById(clubId);
        }
        public void PrintSquad(Squad squad)
        {
            Console.WriteLine($"{"№",-3} {"Имя",-40} {"Позиция",-14} {"Возраст",-8} {"Дата рождения",-10} {"Рост",-7} {"Вес",-7} {"Рабочая нога",-10}");
            Console.WriteLine("Вратари:");
            foreach (var player in squad.Goalkeepers)
            {
                if (player.Number != null)
                    Console.Write($"{player.Number,-3} ");
                else
                    Console.Write($"{"",-3} ");
                if (player.Name != null)
                    Console.Write($"{player.Name,-40} ");
                else
                    Console.Write($"{"",-40} ");
                if (player.Position != null)
                    Console.Write($"{player.Position,-14} ");
                else
                    Console.Write($"{"",-14} ");
                if (player.Age != null)
                    Console.Write($"{player.Age,-8} ");
                else
                    Console.Write($"{"",-8} ");
                if (player.DateOfBirth != null)
                {
                    DateTime date = player.DateOfBirth ?? DateTime.Now;
                    Console.Write($"{date.ToString("d"),-13} ");
                }
                else
                    Console.Write($"{"",-13} ");
                if (player.Height != null)
                    Console.Write($"{player.Height,3} см ");
                else
                    Console.Write($"{"",-7} ");
                if (player.Weight != null)
                    Console.Write($"{player.Weight,3} кг ");
                else
                    Console.Write($"{"",-6} ");
                if (player.WorkingLeg != null)
                    Console.Write($"{player.WorkingLeg,10} ");
                else
                    Console.Write($"{"",-3} ");
                Console.WriteLine();
            }
            Console.WriteLine("Защитники:");
            foreach (var player in squad.Defenders)
            {
                if (player.Number != null)
                    Console.Write($"{player.Number,-3} ");
                else
                    Console.Write($"{"",-3} ");
                if (player.Name != null)
                    Console.Write($"{player.Name,-40} ");
                else
                    Console.Write($"{"",-40} ");
                if (player.Position != null)
                    Console.Write($"{player.Position,-14} ");
                else
                    Console.Write($"{"",-14} ");
                if (player.Age != null)
                    Console.Write($"{player.Age,-8} ");
                else
                    Console.Write($"{"",-8} ");
                if (player.DateOfBirth != null)
                {
                    DateTime date = player.DateOfBirth ?? DateTime.Now;
                    Console.Write($"{date.ToString("d"),-13} ");
                }
                else
                    Console.Write($"{"",-13} ");
                if (player.Height != null)
                    Console.Write($"{player.Height,3} см ");
                else
                    Console.Write($"{"",-7} ");
                if (player.Weight != null)
                    Console.Write($"{player.Weight,3} кг ");
                else
                    Console.Write($"{"",-6} ");
                if (player.WorkingLeg != null)
                    Console.Write($"{player.WorkingLeg,10} ");
                else
                    Console.Write($"{"",-3} ");
                Console.WriteLine();
            }
            Console.WriteLine("Полузащитники:");
            foreach (var player in squad.Midfielders)
            {
                if (player.Number != null)
                    Console.Write($"{player.Number,-3} ");
                else
                    Console.Write($"{"",-3} ");
                if (player.Name != null)
                    Console.Write($"{player.Name,-40} ");
                else
                    Console.Write($"{"",-40} ");
                if (player.Position != null)
                    Console.Write($"{player.Position,-14} ");
                else
                    Console.Write($"{"",-14} ");
                if (player.Age != null)
                    Console.Write($"{player.Age,-8} ");
                else
                    Console.Write($"{"",-8} ");
                if (player.DateOfBirth != null)
                {
                    DateTime date = player.DateOfBirth ?? DateTime.Now;
                    Console.Write($"{date.ToString("d"),-13} ");
                }
                else
                    Console.Write($"{"",-13} ");
                if (player.Height != null)
                    Console.Write($"{player.Height,3} см ");
                else
                    Console.Write($"{"",-7} ");
                if (player.Weight != null)
                    Console.Write($"{player.Weight,3} кг ");
                else
                    Console.Write($"{"",-6} ");
                if (player.WorkingLeg != null)
                    Console.Write($"{player.WorkingLeg,10} ");
                else
                    Console.Write($"{"",-3} ");
                Console.WriteLine();
            }
            Console.WriteLine("Нападающие:");
            foreach (var player in squad.Attackers)
            {
                if (player.Number != null)
                    Console.Write($"{player.Number,-3} ");
                else
                    Console.Write($"{"",-3} ");
                if (player.Name != null)
                    Console.Write($"{player.Name,-40} ");
                else
                    Console.Write($"{"",-40} ");
                if (player.Position != null)
                    Console.Write($"{player.Position,-14} ");
                else
                    Console.Write($"{"",-14} ");
                if (player.Age != null)
                    Console.Write($"{player.Age,-8} ");
                else
                    Console.Write($"{"",-8} ");
                if (player.DateOfBirth != null)
                {
                    DateTime date = player.DateOfBirth ?? DateTime.Now;
                    Console.Write($"{date.ToString("d"),-13} ");
                }
                else
                    Console.Write($"{"",-13} ");
                if (player.Height != null)
                    Console.Write($"{player.Height,3} см ");
                else
                    Console.Write($"{"",-7} ");
                if (player.Weight != null)
                    Console.Write($"{player.Weight,3} кг ");
                else
                    Console.Write($"{"",-6} ");
                if (player.WorkingLeg != null)
                    Console.Write($"{player.WorkingLeg,10} ");
                else
                    Console.Write($"{"",-3} ");
                Console.WriteLine();
            }
        }
        public CompetitionsDetails GetCompetitionDetailsById(string id, string season)
        {
            string htmlCode = GetHTMLInfo(id, SearchScope.competitions, season + "/");
            int indexStartInfo = htmlCode.LastIndexOf("<tbody>");
            int indexEndInfo = htmlCode.IndexOf("</tbody>", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            Match matchName = Regex.Match(htmlCode, @"profile_info_title red"" > ([^<] +)");
            Match matchCountry = Regex.Match(htmlCode, @"<span>([^<]+)<\/span>");
            Match matchDate = Regex.Match(htmlCode, @"<\/td><td>([0-9\.]+) - ([0-9\.]+)<\/td><\/tr>");
            Match matchAttendance = Regex.Match(htmlCode, @"<\/td><td>([0-9]+)<\/td><\/tr>");
            Match matchMatches = Regex.Match(htmlCode, @"<\/td><td>([0-9]+) из ([0-9]+)<\/td><\/tr>");
            Match matchStage = Regex.Match(htmlCode, @"Стадия<\/td><[^>]+>([^<]+)");
            string country = null, currentStage = null, name = null;
            DateTime? dateStart = null, dateEnd = null;
            int? attendance = null, matchesPlayed = null, matchesAll = null;
            if (matchName.Success)
                name = matchName.Groups[1].Value;
            if (matchCountry.Success)
                country = matchCountry.Groups[1].Value;
            if (matchDate.Success)
            {
                dateStart = DateTime.Parse(matchDate.Groups[1].Value);
                dateEnd = DateTime.Parse(matchDate.Groups[2].Value);
            }
            if (matchAttendance.Success)
                attendance = int.Parse(matchAttendance.Groups[1].Value);
            if (matchMatches.Success)
            {
                matchesPlayed = int.Parse(matchMatches.Groups[1].Value);
                matchesAll = int.Parse(matchMatches.Groups[2].Value);
            }
            if (matchStage.Success)
                currentStage = matchStage.Groups[1].Value;
            CompetitionSingleTable singleTable = GetCompetitionSignleTableById(id, season);
            CompetitionGroupTables groupTables = GetCompetitionGroupTableById(id, season);
            CompetitionsDetails competitions = new CompetitionsDetails(id, name, season, currentStage, country, dateStart, dateEnd, attendance, matchesPlayed, matchesAll, singleTable, groupTables);
            return competitions;
        }
        public CompetitionsDetails GetCompetitionsDetailsByName(string name, string country, string season)
        {
            string id = GetCompetitionIdByName(name, country);
            return GetCompetitionDetailsById(id, season);
        }
        public CompetitionSingleTable GetCompetitionSignleTableById(string id, string season)
        {
            string htmlCode = GetHTMLInfo(id, SearchScope.competitions, season + "/");
            int indexStartInfo = htmlCode.IndexOf("competition_table");
            if (indexStartInfo == -1)
                return new CompetitionSingleTable();
            int indexEndInfo = htmlCode.IndexOf("</table>");
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
            indexStartInfo = 0;
            List<RowInCompetitionTable> table = new List<RowInCompetitionTable>();
            while (htmlCode.IndexOf("<tr>", indexStartInfo) != -1)
            {
                indexStartInfo = htmlCode.IndexOf("<tr>", indexStartInfo);
                indexEndInfo = htmlCode.IndexOf("</tr>", indexStartInfo);
                string htmlCodeRow = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);
                Match matchPosition = Regex.Match(htmlCodeRow, @"([0-9]+)<\/div><\/td>");
                Match matchClub = Regex.Match(htmlCodeRow, @"href=""\/clubs\/([0-9]+)\/"">([^<]+)");
                Match matchStats = Regex.Match(htmlCodeRow, @"<td class=""ctr"">([^<]+)<\/td>");
                Match matchPoints = Regex.Match(htmlCodeRow, @">([0-9]+)(<\/b>|<\/span>)");
                FootballClub club = null;
                int position = default;
                if (matchPosition.Success)
                {
                    position = int.Parse(matchPosition.Groups[1].Value);
                }
                if (matchClub.Success)
                {
                    string clubId = matchClub.Groups[1].Value,
                        clubName = matchClub.Groups[2].Value;
                    club = new FootballClub(clubId, clubName);
                }
                int[] sts = new int[7];
                int i = 0;
                while (matchStats.Success && i < 7)
                {
                    sts[i] = int.Parse(matchStats.Groups[1].Value);
                    matchStats = matchStats.NextMatch();
                    i++;
                }
                int points = default;
                if (matchPoints.Success)
                {
                    points = int.Parse(matchPoints.Groups[1].Value);
                }
                RowInCompetitionTable row = new RowInCompetitionTable(position, club, sts[0], sts[1], sts[2], sts[3], sts[4], sts[5], sts[6], points);
                table.Add(row);
                indexStartInfo = indexEndInfo + 3;
                matchClub = matchClub.NextMatch();
                matchPoints = matchPoints.NextMatch();
                matchPosition = matchPosition.NextMatch();
            }
            return new CompetitionSingleTable(id, season, table);
        }
        public CompetitionGroupTables GetCompetitionGroupTableById(string id, string season)
        {
            string htmlCode = GetHTMLInfo(id, SearchScope.competitions, season + "/");
            int indexStartTable = htmlCode.IndexOf("Групповая стадия");
            if (indexStartTable == -1)
                return new CompetitionGroupTables();
            int indexEndTable = htmlCode.IndexOf("live_comptt_bd", indexStartTable);
            htmlCode = htmlCode.Substring(indexStartTable, indexEndTable - indexStartTable);
            indexStartTable = 0;
            List<CompetitionTable> groupTable = new List<CompetitionTable>();
            int j = 0;
            while (htmlCode.IndexOf("<tbody>", indexStartTable) != -1)
            {
                indexStartTable = htmlCode.IndexOf("<tbody>", indexStartTable);
                indexEndTable = htmlCode.IndexOf("</tbody>", indexStartTable);
                string htmlCodeTable = htmlCode.Substring(indexStartTable, indexEndTable - indexStartTable);
                int indexStartRow = 0;
                int indexEndRow = 0;
                List<RowInCompetitionTable> table = new List<RowInCompetitionTable>();
                while (htmlCodeTable.IndexOf("<tr>", indexStartRow) != -1)
                {
                    indexStartRow = htmlCodeTable.IndexOf("<tr>", indexStartRow);
                    indexEndRow = htmlCodeTable.IndexOf("</tr>", indexStartRow);
                    string htmlCodeRow = htmlCodeTable.Substring(indexStartRow, indexEndRow - indexStartRow);
                    Match matchPosition = Regex.Match(htmlCodeRow, @"([0-9]+)<\/div><\/td>");
                    Match matchClub = Regex.Match(htmlCodeRow, @"href=""\/clubs\/([0-9]+)\/"">([^<]+)");
                    Match matchStats = Regex.Match(htmlCodeRow, @"<td class=""ctr"">([^<]+)<\/td>");
                    Match matchPoints = Regex.Match(htmlCodeRow, @">([0-9]+)(<\/b>|<\/span>)");
                    FootballClub club = null;
                    int position = default;
                    if (matchPosition.Success)
                    {
                        position = int.Parse(matchPosition.Groups[1].Value);
                    }
                    if (matchClub.Success)
                    {
                        string clubId = matchClub.Groups[1].Value,
                            clubName = matchClub.Groups[2].Value;
                        club = new FootballClub(clubId, clubName);
                    }
                    int[] sts = new int[7];
                    int i = 0;
                    while (matchStats.Success && i < 7)
                    {
                        sts[i] = int.Parse(matchStats.Groups[1].Value);
                        matchStats = matchStats.NextMatch();
                        i++;
                    }
                    int points = default;
                    if (matchPoints.Success)
                    {
                        points = int.Parse(matchPoints.Groups[1].Value);
                    }
                    RowInCompetitionTable row = new RowInCompetitionTable(position, club, sts[0], sts[1], sts[2], sts[3], sts[4], sts[5], sts[6], points);
                    table.Add(row);
                    indexStartRow = indexEndRow + 3;
                    matchClub = matchClub.NextMatch();
                    matchPoints = matchPoints.NextMatch();
                    matchPosition = matchPosition.NextMatch();
                }
                groupTable.Add(new CompetitionTable(Convert.ToChar(65 + j), table));
                indexStartTable = indexEndTable + 3;
                j++;
            }
            return new CompetitionGroupTables(id, season, groupTable);
        }
        //UNDONE: поддерживает только основные чемпионаты
        public CompetitionSingleTable GetCompetitionsSingleTableByName(string name, string country, string season)
        {
            string id = GetCompetitionIdByName(name, country);
            return GetCompetitionSignleTableById(id, season);
        }
        public CompetitionGroupTables GetCompetitionsGroupTableByName(string name, string country, string season)
        {
            string id = GetCompetitionIdByName(name, country);
            return GetCompetitionGroupTableById(id, season);
        }
        public void PrintRowInCompetitionTable(RowInCompetitionTable row)
        {
            Console.WriteLine($"{"№",-4} {"Команда",-40} {"Игр",-4} {"Вгр",-4} {"Нч",-4} {"Прж",-4} {"Заб",-4} {"Прп",-4} {"+/-",-4} {"О",-4}");
            Console.WriteLine($"{row.Position,-4} {row.Club.Name,-40} {row.GamesPlayed,-4} {row.GamesWon,-4} {row.GamesDrawn,-4} {row.GamesLost,-4} {row.GoalsScored,-4} {row.GoalsMissed,-4} " +
                    $"{row.GoalsDifference,-4} {row.Points,-4}");
        }
        public void PrintCompetitionSingleTable(CompetitionSingleTable table)
        {
            Console.WriteLine($"Id: {table.Id}");
            Console.WriteLine($"Сезон: {table.Season}");
            Console.WriteLine($"{"№",-4} {"Команда",-40} {"Игр",-4} {"Вгр",-4} {"Нч",-4} {"Прж",-4} {"Заб",-4} {"Прп",-4} {"+/-", -4} {"О", -4}");
            foreach(var row in table.Table)
            {
                Console.WriteLine($"{row.Position,-4} {row.Club.Name,-40} {row.GamesPlayed,-4} {row.GamesWon,-4} {row.GamesDrawn,-4} {row.GamesLost,-4} {row.GoalsScored,-4} {row.GoalsMissed,-4} " +
                    $"{row.GoalsDifference,-4} {row.Points,-4}");
            }
        }
        public void PrintCompetitionGroupTable(CompetitionGroupTables groupTable)
        {
            Console.WriteLine($"Id: {groupTable.Id}");
            Console.WriteLine($"Сезон: {groupTable.Season}");
            int i = 0;
            foreach (var table in groupTable.GroupTable)
            {
                Console.WriteLine($"Группа {Convert.ToChar(65 + i)}:");
                Console.WriteLine($"{"№",-4} {"Команда",-40} {"Игр",-4} {"Вгр",-4} {"Нч",-4} {"Прж",-4} {"Заб",-4} {"Прп",-4} {"+/-",-4} {"О",-4}");
                foreach (var row in table.Table)
                {
                    Console.WriteLine($"{row.Position,-4} {row.Club.Name,-40} {row.GamesPlayed,-4} {row.GamesWon,-4} {row.GamesDrawn,-4} {row.GamesLost,-4} {row.GoalsScored,-4} {row.GoalsMissed,-4} " +
    $"{row.GoalsDifference,-4} {row.Points,-4}");
                }
                i++;
            }
        }
        public SquadPlayersStatistics GetSquadPlayersStatisticsById(string clubId)
        {
            string htmlCode = GetHTMLInfo(clubId, SearchScope.clubs, "&tab=players");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlCode);
            var table = doc.DocumentNode.SelectSingleNode(".//table[@id='players_all']/tbody");
            var rows = table.SelectNodes(".//tr");
            List <RowSquadPlayerStatistics> playerStatistics = new List<RowSquadPlayerStatistics>();
            foreach(var row in rows)
            {
                var hrefCell = row.SelectSingleNode(".//a[@class='name']");
                var playerCell = row.SelectSingleNode(".//td");
                string name = null, position = null, firstName = null, lastName = null, playerId = null;
                int? number = null;
                if (hrefCell != null)
                {
                    string hrefStr = hrefCell.GetAttributeValue("href", "");
                    Match matchPlayerId = Regex.Match(hrefStr, @".*/(\d+)/");
                    if (matchPlayerId.Groups[1].Success)
                        playerId = matchPlayerId.Groups[1].Value;
                }
                if (playerCell != null)
                {
                    Match matchPlayer = Regex.Match(playerCell.InnerHtml, @">([^<]+)</span>");
                    Match matchPlayerNumber = Regex.Match(playerCell.InnerHtml, @"<br>#(\d+)");
                    Match matchPlayerPosition = Regex.Match(playerCell.InnerHtml, @"([А-Яа-я]+)</div>");
                    if (matchPlayer.Success)
                    {
                        name = matchPlayer.Groups[1].Value;
                        firstName = name.Split(' ')[0];
                        lastName = string.Join(' ', name.Split(' ').Skip(1));
                    }
                    if (matchPlayerNumber.Success)
                        number = int.Parse(matchPlayerNumber.Groups[1].Value);
                    if (matchPlayerPosition.Success)
                        position = matchPlayerPosition.Groups[1].Value;
                }
                Person player = new Person(playerId, firstName, lastName);
                List<int> stats = new List<int>();
                var statsCells = row.SelectNodes(".//td").Skip(1);
                
                foreach (var cell in statsCells)
                {
                    if (cell.InnerText != "&nbsp;")
                        stats.Add(int.Parse(cell.InnerText));
                    else
                        stats.Add(0);
                }
                if (stats.All(x => x == 0))
                    continue;
                var rowSquadPlayer = new RowSquadPlayerStatistics(player, number, position, stats[0], stats[1], stats[2],
                        stats[3], stats[4], stats[5], stats[6], stats[7], stats[8], stats[9], stats[10], stats[11], stats[12]);
                playerStatistics.Add(rowSquadPlayer);
            }
            var seasonPath = doc.DocumentNode.SelectSingleNode(".//div[@class='selectboxes mb10']/div[@class='selectbox lh16']/span[@class='selectbox-label']");
            string season = seasonPath.InnerText;
            var competitionPath = doc.DocumentNode.SelectSingleNode(".//div[@class='selectboxes mb10']/div[@class='selectbox lh16']/span[@class='selectbox-label']/span[@class='flag16']");
            string competiton = competitionPath.InnerText;
            var squadStatistics = new SquadPlayersStatistics(clubId, season, new Competitions(competiton), playerStatistics);
            return squadStatistics;
        }
        public SquadPlayersStatistics GetSquadPlayersStatisticsByName(string clubName)
        {
            string id = GetClubIdByName(clubName);
            return GetSquadPlayersStatisticsById(id);
        }
        public void PrintSquadPlayersStatistics(SquadPlayersStatistics table)
        {
            Console.WriteLine($"Турнир: {table.Competition} Сезон: {table.Season}");
            Console.WriteLine();
            Console.WriteLine($"{"Имя",-30} {"Г", 3} {"П",3} {"М",3} {"МИН",4} {"Г+П",3} {"ПЕН",3} {"ДБЛ",3} {"ХТК",3} {"АГ",3} {"ЖК",3} {"ЖКК",3} {"КК",3} {"ФПБ",3}");
            Console.WriteLine();
            foreach (var stat in table.PlayerStatistics) {
                Console.WriteLine($"{stat.Player.Name,-30} {stat.Goals,3} {stat.Assists,3} {stat.Matches,3} {stat.Minutes,-4} {stat.GoalPlusPass,3} {stat.PenGoals,3} " +
                        $"{stat.DoubleGoals,3} {stat.HatTricks,3} {stat.AutoGoals,3} {stat.YellowCards,3} {stat.YellowRedCards,3} {stat.RedCards,3} {stat.FairPlayScore,3}");
                Console.WriteLine($"#{stat.Number + " " + stat.Position,-30}");
                Console.WriteLine();
            }
        }
        private string GetCompetitionPlayerStatisticsHTML(string cp_ss, string size, string page, SortScope scope, SortOrder sortOrder)
        {
            string address = $"https://soccer365.ru/?c=competitions&a=tab_tablesorter_players&cp_ss={cp_ss}&cl=0&page={page}&size={size}&col[{(int)scope}]={(int)sortOrder}";
            return restClient.GetStringAsync(address).Result;
        }
        public CompetitionPlayersStatistics GetCompetitionPlayerStatisticsById(string competitionId, string season = "", string size = "25", string page = "0", SortScope scope = SortScope.goals, SortOrder sortOrder = SortOrder.DESC)
        {
            string htmlCode = GetHTMLInfo(competitionId, SearchScope.competitions, $"{season}/players/");
            if (season == "")
                season = Regex.Match(htmlCode, @"selectbox-label"">([^<]+)").Groups[1].Value;
            string cp_ss = Regex.Match(htmlCode, @"cp_ss=([\d]+)").Groups[1].Value;
            htmlCode = GetCompetitionPlayerStatisticsHTML(cp_ss, size, page, scope, sortOrder);
            JObject json = JObject.Parse(htmlCode);
            var playersStats = new List<RowCompetitionPlayerStatistics>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            foreach (var row in json["rows"])
            {
                string htmlNameInfo = row[0].ToString();
                doc.LoadHtml(htmlNameInfo);
                var hrefCell = doc.DocumentNode.SelectSingleNode(".//a[@class='name']");
                FootballClub club = null;
                Person player = null;
                string position = null;
                int? number = null;
                if (hrefCell != null)
                {
                    string firstName = null, lastName = null, playerId = null;
                    string hrefStr = hrefCell.GetAttributeValue("href", "");
                    Match matchPlayerId = Regex.Match(hrefStr, @".*/(\d+)/");
                    if (matchPlayerId.Groups[1].Success)
                        playerId = matchPlayerId.Groups[1].Value;
                    firstName = hrefCell.InnerText.Split(' ')[0];
                    lastName = string.Join(' ', hrefCell.InnerText.Split(' ').Skip(1));
                    Match matchPlayerNumber = Regex.Match(htmlNameInfo, @"<br/>#(\d+)");
                    Match matchPlayerPosition = Regex.Match(htmlNameInfo, @"([А-Яа-я]+)</div>");
                    if (matchPlayerNumber.Success)
                        number = int.Parse(matchPlayerNumber.Groups[1].Value);
                    if (matchPlayerPosition.Success)
                        position = matchPlayerPosition.Groups[1].Value;
                    player = new Person(playerId, firstName, lastName);
                }
                Match matchClubInfo = Regex.Match(htmlNameInfo, @"clubs/([\d]+)/"" title=""([^""]+)");
                if (matchClubInfo.Success)
                    club = new FootballClub(matchClubInfo.Groups[1].Value, matchClubInfo.Groups[2].Value);
                var stats = Array.ConvertAll(row.Skip(1).ToArray(), x => { if (x.ToString() != "&nbsp;") return (int)x; else return 0; });
                RowCompetitionPlayerStatistics playerStats = new RowCompetitionPlayerStatistics(club, player, number, position, stats[0], stats[1], stats[2], stats[3], stats[4],
                    stats[5], stats[6], stats[7], stats[8], stats[9], stats[10], stats[11], stats[12]);
                playersStats.Add(playerStats);
            }
            Competitions competition = GetCompetitionById(competitionId);
            competition.Season = season;
            var competitionPlayersStatistics = new CompetitionPlayersStatistics(competition, playersStats);
            return competitionPlayersStatistics;
        }
        public CompetitionPlayersStatistics GetCompetitionPlayerStatisticsByName(string name, string country, string season = "", string size = "25", string page = "0", SortScope scope = SortScope.goals, SortOrder sortOrder = SortOrder.DESC)
        {
            string id = GetCompetitionIdByName(name, country);
            return GetCompetitionPlayerStatisticsById(id, season, size, page, scope, sortOrder);
        }
        public void PrintCompetitionPlayersStatistics(CompetitionPlayersStatistics table)
        {
            Console.WriteLine($"Турнир: {table.Competition.Name} Сезон: {table.Competition.Season}");
            Console.WriteLine();
            Console.WriteLine($"{"Имя",-30} {"Клуб", -30} {"Г",3} {"П",3} {"М",3} {"МИН",4} {"Г+П",3} {"ПЕН",3} {"ДБЛ",3} {"ХТК",3} {"АГ",3} {"ЖК",3} {"ЖКК",3} {"КК",3} {"ФПБ",3}");
            Console.WriteLine();
            foreach (var stat in table.PlayerStatistics)
            {
                Console.WriteLine($"{stat.Player.Name,-30} {stat.Club.Name, -30} {stat.Goals,3} {stat.Assists,3} {stat.Matches,3} {stat.Minutes,-4} {stat.GoalPlusPass,3} {stat.PenGoals,3} " +
                        $"{stat.DoubleGoals,3} {stat.HatTricks,3} {stat.AutoGoals,3} {stat.YellowCards,3} {stat.YellowRedCards,3} {stat.RedCards,3} {stat.FairPlayScore,3}");
                Console.WriteLine($"#{stat.Number + " " + stat.Position,-30}");
                Console.WriteLine();
            }
        }
    }
}
