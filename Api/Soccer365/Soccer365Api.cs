using Rest;
using Soccer365.Model;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace Soccer365
{
    public class Soccer365Api
    {
        private readonly RestClient restClient = new RestClient();

        public List<FootballMatch> GetTodayMatches()
        {
            string htmlCode = GetHTMLTodayMatches();
            string patternClubs = @"title=""([^""]* - [^""]*)"">";
            string patternStartDate = @"""status"".*>([0-9А-Яа-я':.,\- ]+)<.*\/div>";
            string patternScore = @"<div class=""gls"">([0-9\-]+)<\/div>";
            Regex regexClubs = new Regex(patternClubs);
            Regex regexStartDate = new Regex(patternStartDate);
            Regex regexScore = new Regex(patternScore);
            Match matchClubs = regexClubs.Match(htmlCode);
            Match matchStartDate = regexStartDate.Match(htmlCode);
            Match matchScore = regexScore.Match(htmlCode);

            var matches = new List<FootballMatch>();
            while (matchClubs.Success || matchStartDate.Success || matchScore.Success)
            {
                var score = matchScore.Groups[1].Value;
                matchScore = matchScore.NextMatch();
                score += $":{matchScore.Groups[1].Value}";

                var match = new FootballMatch()
                {
                    StartDate = matchStartDate.Groups[1].Value,
                    ClubHome = matchClubs.Groups[1].Value.Split('-')[0],
                    ClubAway = matchClubs.Groups[1].Value.Split('-')[1],
                    Score = score,
                };
                matches.Add(match);

                matchScore = matchScore.NextMatch();
                matchClubs = matchClubs.NextMatch();
                matchStartDate = matchStartDate.NextMatch();
            }

            return matches;
        }

        private string GetHTMLTodayMatches()
        {
            return restClient.GetStringAsync("https://soccer365.ru/online/").Result;
        }   
    }
}
