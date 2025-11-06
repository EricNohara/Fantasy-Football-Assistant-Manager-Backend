using CsvHelper.Configuration;
using Fantasy_Football_Assistant_Manager.Models.Csv;

namespace Fantasy_Football_Assistant_Manager.Mappings;

public class TeamDataCsvMap: ClassMap<TeamDataCsv>
{
    public TeamDataCsvMap()
    {
        Map(m => m.Id).Name("team_abbr");
        Map(m => m.Name).Name("team_name");
        Map(m => m.Conference).Name("team_conf");
        Map(m => m.Division).Name("team_division");
        Map(m => m.LogoUrl).Name("team_logo_espn");
    }
}
