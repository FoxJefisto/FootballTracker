using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class FootballClubDetails : FootballClub
    {
        public FootballClubDetails(string clubId, string name, string englishName,string fullName, string mainCoach, string stadium, string city, string country, string foundationDate, int? rating, List<Competitions> competitions) : base(clubId, name)
        {
            NameEnglish = englishName;
            FullName = fullName;
            MainCoach = mainCoach;
            Stadium = stadium;
            City = city;
            Country = country;
            FoundationDate = foundationDate;
            Rating = rating;
            Competitions = competitions;
        }
        public string NameEnglish { get; private set; }
        public string FullName { get; private set; }
        public string MainCoach { get; private set; }
        public string City { get; private set; }
        public string Country { get; private set; }
        public string Stadium { get; private set; }
        public string FoundationDate { get; private set; }
        public int? Rating { get; private set; }
        public List<Competitions> Competitions { get; private set; }
        //TODO: Добавить списки с последними результатами
        //public List<FootballMatchDetails> MatchShedule { get; private set; }
        //public List<FootballMatchDetails> MatchResults { get; private set; }

    }
}
