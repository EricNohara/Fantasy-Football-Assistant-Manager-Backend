using CsvHelper.Configuration;
using Fantasy_Football_Assistant_Manager.Models;

namespace Fantasy_Football_Assistant_Manager.Mappings;

public class PlayerStatCsvMap: ClassMap<PlayerStatCsv>
{
    public PlayerStatCsvMap()
    {
        // Basic info
        Map(m => m.PlayerId).Name("player_id");
        Map(m => m.Position).Name("position");

        // optional fields
        Map(m => m.Week).Name("week").Optional();
        Map(m => m.SeasonStartYear).Name("season").Optional();

        Map(m => m.Completions).Name("completions");
        Map(m => m.PassingAttempts).Name("attempts");
        Map(m => m.PassingYards).Name("passing_yards");
        Map(m => m.PassingTds).Name("passing_tds");
        Map(m => m.InterceptionsAgainst).Name("passing_interceptions");
        Map(m => m.SacksAgainst).Name("sacks_suffered");
        Map(m => m.FumblesAgainst).Name("sack_fumbles_lost");
        Map(m => m.PassingFirstDowns).Name("passing_first_downs");
        Map(m => m.PassingEpa).Name("passing_epa");

        Map(m => m.Carries).Name("carries");
        Map(m => m.RushingYards).Name("rushing_yards");
        Map(m => m.RushingTds).Name("rushing_tds");
        Map(m => m.RushingFirstDowns).Name("rushing_first_downs");
        Map(m => m.RushingEpa).Name("rushing_epa");

        Map(m => m.Receptions).Name("receptions");
        Map(m => m.Targets).Name("targets");
        Map(m => m.ReceivingYards).Name("receiving_yards");
        Map(m => m.ReceivingTds).Name("receiving_tds");
        Map(m => m.ReceivingFirstDowns).Name("receiving_first_downs");
        Map(m => m.ReceivingEpa).Name("receiving_epa");

        Map(m => m.FgMadeList).Name("fg_made_list");
        Map(m => m.FgMissedList).Name("fg_missed_list");
        Map(m => m.FgBlockedList).Name("fg_blocked_list");

        Map(m => m.PadAttempts).Name("pat_att");
        Map(m => m.PatPercent).Name("pat_pct");

        Map(m => m.FantasyPoints).Name("fantasy_points");
        Map(m => m.FantasyPointsPpr).Name("fantasy_points_ppr");
    }
}
