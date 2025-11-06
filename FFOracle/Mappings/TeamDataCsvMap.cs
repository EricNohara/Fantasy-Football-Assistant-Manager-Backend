using CsvHelper.Configuration;
using FFOracle.Models.Csv;

namespace FFOracle.Mappings;

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
