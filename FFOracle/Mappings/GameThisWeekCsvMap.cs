using CsvHelper.Configuration;
using FFOracle.Models.Csv;
using System.Globalization;

namespace FFOracle.Mappings;

public class GameThisWeekCsvMap: ClassMap<GameThisWeekCsv>
{
    public GameThisWeekCsvMap()
    {
        Map(m => m.HomeTeam).Name("home_team");
        Map(m => m.AwayTeam).Name("away_team");
        Map(m => m.Weekday).Name("weekday");
        Map(m => m.Season).Name("season");
        Map(m => m.Week).Name("week");

        // combine gameday + gametime into one DateTime
        Map(m => m.GameDateTime)
            .Convert(args =>
            {
                var day = args.Row.GetField("gameday");
                var time = args.Row.GetField("gametime");

                if (DateTime.TryParseExact(
                        $"{day} {time}",
                        "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var result))
                {
                    return result;
                }

                return null;
            });

        Map(m => m.StadiumName).Name("stadium");
        Map(m => m.StadiumStyle).Name("roof");

        // map 1 and 0 to boolean
        Map(m => m.IsDivisionalGame)
            .Name("div_game")
            .Convert(args =>
            {
                var value = args.Row.GetField("div_game");
                if (string.IsNullOrWhiteSpace(value)) return false;
                return value.Trim() == "1";
            });

        // Rest days
        Map(m => m.HomeRestDays).Name("home_rest");
        Map(m => m.AwayRestDays).Name("away_rest");

        // Betting odds
        Map(m => m.HomeMoneyline).Name("home_moneyline");
        Map(m => m.AwayMoneyline).Name("away_moneyline");
        Map(m => m.HomeSpreadOdds).Name("home_spread_odds");
        Map(m => m.AwaySpreadOdds).Name("away_spread_odds");
        Map(m => m.SpreadLine).Name("spread_line");
        Map(m => m.TotalLine).Name("total_line");
        Map(m => m.UnderOdds).Name("under_odds");
        Map(m => m.OverOdds).Name("over_odds");
    }
}
