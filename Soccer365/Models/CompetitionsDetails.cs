using System;
using System.Collections.Generic;
using System.Text;

namespace Soccer365.Models
{
    public class CompetitionsDetails : Competitions
    {
        public enum CompetitionType
        {
            SingleTable,
            GroupTables,
            NotTables
        }
        public CompetitionsDetails(string id, string name, string season, string currentStage, string country, DateTime? dateStart, DateTime? dateEnd,
            int? attendance, int? matchesPlayed, int? matchesAll) : base(id, name, season, country, currentStage)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;
            Attendance = attendance;
            MatchesPlayed = matchesPlayed;
            MatchesAll = matchesAll;
        }

        public CompetitionsDetails(string id, string name, string season, string currentStage, 
            string country, DateTime? dateStart, DateTime? dateEnd,
            int? attendance, int? matchesPlayed, int? matchesAll,
            CompetitionSingleTable singleTable, CompetitionGroupTables groupTables) : this(id, name, season, currentStage, country, dateStart, dateEnd, attendance, matchesPlayed, matchesAll)
        {
            SingleTable = singleTable;
            GroupTables = groupTables;
            CompType = GetCompType();
        }

        private CompetitionType GetCompType()
        {
            CompetitionType result;
            if (SingleTable.Table.Count != 0)
                result = CompetitionType.SingleTable;
            else if (GroupTables.GroupTable.Count != 0)
                result = CompetitionType.GroupTables;
            else
                result = CompetitionType.NotTables;
            return result;
        }
        public DateTime? DateStart { get; private set; }
        public DateTime? DateEnd { get; private set; }
        public int? Attendance { get; private set; }
        public int? MatchesPlayed { get; private set; }
        public int? MatchesAll { get; private set; }
        public CompetitionType CompType { get; private set; }
        private CompetitionSingleTable SingleTable { get; set; }
        private CompetitionGroupTables GroupTables { get; set; }
    }
}
