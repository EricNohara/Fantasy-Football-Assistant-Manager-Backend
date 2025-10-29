using CsvHelper.Configuration;
using Fantasy_Football_Assistant_Manager.Models;

namespace Fantasy_Football_Assistant_Manager.Mappings;

public class TeamSeasonStatCsvMap: ClassMap<TeamSeasonStatCsv>
{
    public TeamSeasonStatCsvMap()
    {
        Map(m => m.Team).Name("team");

        // DEFENSE
        Map(m => m.TacklesForLoss).Name("def_tackles_for_loss");
        Map(m => m.TacklesForLossYards).Name("def_tackles_for_loss_yards");
        Map(m => m.FumblesFor).Name("def_fumbles_forced");
        Map(m => m.SacksFor).Name("def_sacks");
        Map(m => m.SackYardsFor).Name("def_sack_yards");
        Map(m => m.InterceptionsFor).Name("def_interceptions");
        Map(m => m.InterceptionYardsFor).Name("def_interceptions_yards");
        Map(m => m.DefTds).Name("def_tds");
        Map(m => m.Safeties).Name("def_safeties");
        Map(m => m.PassDefended).Name("def_pass_defended");

        // OFFENSE
        Map(m => m.Completions).Name("completions");
        Map(m => m.Attempts).Name("attempts");
        Map(m => m.PassingYards).Name("passing_yards");
        Map(m => m.PassingTds).Name("passing_tds");
        Map(m => m.PassingInterceptions).Name("passing_interceptions");
        Map(m => m.SacksAgainst).Name("sacks_suffered");
        // calculate total fumbles against
        Map(m => m.FumblesAgainst)
            .Convert(row =>
            {
                var sack = row.Row.GetField<int?>("sack_fumbles_lost") ?? 0;
                var rush = row.Row.GetField<int?>("rushing_fumbles_lost") ?? 0;
                var recv = row.Row.GetField<int?>("receiving_fumbles_lost") ?? 0;
                return sack + rush + recv;
            });
        Map(m => m.Carries).Name("carries");
        Map(m => m.RushingYards).Name("rushing_yards");
        Map(m => m.RushingTds).Name("rushing_tds");
        Map(m => m.Receptions).Name("receptions");
        Map(m => m.Targets).Name("targets");
        Map(m => m.ReceivingYards).Name("receiving_yards");
        Map(m => m.ReceivingTds).Name("receiving_tds");
    }
}
