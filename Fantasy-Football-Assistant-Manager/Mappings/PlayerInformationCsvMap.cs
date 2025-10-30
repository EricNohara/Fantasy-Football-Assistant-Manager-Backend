using CsvHelper.Configuration;
using Fantasy_Football_Assistant_Manager.Models.Csv;

namespace Fantasy_Football_Assistant_Manager.Mappings;

public class PlayerInformationCsvMap : ClassMap<PlayerInformationCsv>
{
    public PlayerInformationCsvMap()
    {
        Map(m => m.Id).Name("gsis_id");
        Map(m => m.LastSeason).Name("last_season");
        Map(m => m.LatestTeam).Name("latest_team");
        Map(m => m.ShortDescription).Name("ngs_status_short_description");
        Map(m => m.Status).Name("status");
    }
}
