namespace FFOracle.Models.Csv;

public class GameThisWeekCsv
{
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int Season { get; set; } = -1;
    public int Week { get; set; } = -1;
    public string? Weekday { get; set; }
    public DateTime? GameDateTime { get; set; }
    public string? StadiumName { get; set; }
    public string? StadiumStyle { get; set; }
    public bool? IsDivisionalGame { get; set; }
    public int? HomeRestDays { get; set; }
    public int? AwayRestDays { get; set; }

    // Betting odds
    public int? HomeMoneyline { get; set; }
    public int? AwayMoneyline { get; set; }
    public int? HomeSpreadOdds { get; set; }
    public int? AwaySpreadOdds { get; set; }
    public double? SpreadLine { get; set; }
    public double? TotalLine { get; set; }
    public int? UnderOdds { get; set; }
    public int? OverOdds { get; set; }
}
