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
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
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
                player = new FootballPlayer(playerId, firstName, lastName, fullName, dateOfBirth, citizenship,
                    placeOfBirth, club, numberInClub, nationalTeam, numberInNatTeam, position, workingLeg, height, weight);
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
            Pair<float> xg = null;
            Pair<int> shots, shotsOnTarget, shotsBlocked, saves, ballPossession, corners, fouls, offsides, yCards, rCards, attacks, attacksDangerous, passes, accPasses, freeKicks, prowing, crosses, tackles;
            shots = shotsOnTarget = shotsBlocked = saves = ballPossession = corners = fouls = offsides = yCards = rCards = attacks = attacksDangerous = passes = accPasses = freeKicks = prowing = crosses = tackles = null;
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
                        accPasses = new Pair<int>(int.Parse(valueInfo1), int.Parse(valueInfo2));
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
                Console.WriteLine($"{"xG:",-21}{st.Xg.OutPair(),-30}");
            if (st.Shots != null)
                Console.WriteLine($"{"Удары:",-21}{st.Shots.OutPair(),-30}");
            if (st.ShotsOnTarget != null)
                Console.WriteLine($"{"Удары в створ:",-21}{st.ShotsOnTarget.OutPair(),-30}");
            if (st.ShotsBlocked != null)
                Console.WriteLine($"{"Блок-но ударов:",-21}{st.ShotsBlocked.OutPair(),-30}");
            if (st.Saves != null)
                Console.WriteLine($"{"Сейвы:",-21}{st.Saves.OutPair(),-30}");
            if (st.BallPossession != null)
                Console.WriteLine($"{"Владение мячом %:",-21}{st.BallPossession.OutPair(),-30}");
            if (st.Corners != null)
                Console.WriteLine($"{"Угловые:",-21}{st.Corners.OutPair(),-30}");
            if (st.Fouls != null)
                Console.WriteLine($"{"Нарушения:",-21}{st.Fouls.OutPair(),-30}");
            if (st.Offsides != null)
                Console.WriteLine($"{"Оффсайды:",-21}{st.Offsides.OutPair(),-30}");
            if (st.YCards != null)
                Console.WriteLine($"{"Желтые карточки:",-21}{st.YCards.OutPair(),-30}");
            if (st.RCards != null)
                Console.WriteLine($"{"Красные карточки:",-21}{st.RCards.OutPair(),-30}");
            if (st.Attacks != null)
                Console.WriteLine($"{"Атаки:",-21}{st.Attacks.OutPair(),-30}");
            if (st.AttacksDangerous != null)
                Console.WriteLine($"{"Опасные атаки:",-21}{st.AttacksDangerous.OutPair(),-30}");
            if (st.Passes != null)
                Console.WriteLine($"{"Передачи:",-21}{st.Passes.OutPair(),-30}");
            if (st.AccPasses != null)
                Console.WriteLine($"{"Точность передач:",-21}{st.AccPasses.OutPair(),-30}");
            if (st.FreeKicks != null)
                Console.WriteLine($"{"Штрафные удары:",-21}{st.FreeKicks.OutPair(),-30}");
            if (st.Prowing != null)
                Console.WriteLine($"{"Вбрасывания:",-21}{st.Prowing.OutPair(),-30}");
            if (st.Crosses != null)
                Console.WriteLine($"{"Навесы:",-21}{st.Crosses.OutPair(),-30}");
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
                Person mainAuthor = new Person(id,firstNameMain, lastNameMain);
                Person secondAuthor = null;
                if(matchEventHome.Groups[2].Value != "")
                {
                    string firstNameSecond = matchEventHome.Groups[2].Value.Split(' ')[0];
                    string lastNameSecond = string.Join(' ', matchEventHome.Groups[2].Value.Split(' ').Skip(1).ToArray());
                    secondAuthor = new Person(firstNameSecond, lastNameSecond);
                }
                MatchMainEvent matchMainEvent = new MatchMainEvent(TeamType.Home, minute, eventName, mainAuthor,secondAuthor);
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
                    Console.WriteLine($"{str, -30} {evnt.Name, -15} {evnt.Minute, 2}");
                }
                if(evnt.Team == TeamType.Away)
                {
                    str += $" {evnt.MainAuthor.FirstName + ' ' + evnt.MainAuthor.LastName}";
                    if (evnt.SecondAuthor != null)
                        str += $" ({evnt.SecondAuthor.FirstName} {evnt.SecondAuthor.LastName})";
                    Console.WriteLine($"{"",47}{evnt.Minute,-6} {evnt.Name,-15} {str,-30}");
                }
            }
        }
        //Сделано
        private Pair<List<MatchPlayer>> GetMatchStartSquad(string htmlCode)
        {
            int indexStartInfo = htmlCode.IndexOf("<div id=\"tm-lineup\">");
            int indexEndInfo = htmlCode.IndexOf("<div id=\"tm-subst\"", indexStartInfo);
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
            int indexStartInfo = htmlCode.IndexOf("<div id=\"tm-subst\"");
            int indexEndInfo = htmlCode.IndexOf("<div id=\"tm-players-position-view\"", indexStartInfo);
            htmlCode = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);

            indexStartInfo = htmlCode.IndexOf("class=\"сomposit_block\"");
            indexEndInfo = htmlCode.IndexOf("<div class=\"сomposit_block\"", indexStartInfo);
            string htmlCodeHome = htmlCode.Substring(indexStartInfo, indexEndInfo - indexStartInfo);

            indexStartInfo = htmlCode.IndexOf("class=\"сomposit_block\"", indexStartInfo);
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
        public void PrintMatchSquads(MatchSquads squads)
        {
            Console.WriteLine($"{"Основной состав:",45}");
            for(int i = 0; i < squads.StartSquad.HomeTeam.Count && i < squads.StartSquad.AwayTeam.Count; i++)
            {
                var playerHome = squads.StartSquad.HomeTeam[i];
                var playerAway = squads.StartSquad.AwayTeam[i];
                Console.WriteLine($"{playerHome.Number, -2} {playerHome.Player.FirstName + " " + playerHome.Player.LastName, -30} " + $"{"",20}" +
                    $"{playerAway.Number,-2} {playerAway.Player.FirstName + " " + playerAway.Player.LastName,-30}");
            }
            Console.WriteLine($"{"Запасные игроки:",45}");
            for (int i = 0; i < squads.ReservePlayers.HomeTeam.Count && i < squads.ReservePlayers.AwayTeam.Count; i++)
            {
                var playerHome = squads.ReservePlayers.HomeTeam[i];
                var playerAway = squads.ReservePlayers.AwayTeam[i];
                Console.WriteLine($"{playerHome.Number,-2} {playerHome.Player.FirstName + " " + playerHome.Player.LastName,-30} " + $"{"",20}" +
                    $"{playerAway.Number,-2} {playerAway.Player.FirstName + " " + playerAway.Player.LastName,-30}");
            }
        }
        //Сделано
        public void PrintAllInfoMatch(MatchMainEvents events, MatchSquads squads, MatchStatistics statistics)
        {
            Console.WriteLine($"ID: {statistics.Id}");
            PrintMatchMainEvents(events);
            PrintMatchSquads(squads);
            PrintMatchStatistics(statistics);
        }
    }
}
