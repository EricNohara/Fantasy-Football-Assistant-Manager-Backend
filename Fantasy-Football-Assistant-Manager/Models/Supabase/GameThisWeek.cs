using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models.Supabase;

[Table("games_this_week")]

public class GameThisWeek: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("home_team")]
    [JsonPropertyName("home_team")]
    public string HomeTeam { get; set; } = string.Empty;

    [Column("away_team")]
    [JsonPropertyName("away_team")]
    public string AwayTeam { get; set; } = string.Empty;

    [Column("weekday")]
    [JsonPropertyName("weekday")]
    public string? Weekday { get; set; }

    [Column("game_datetime")]
    [JsonPropertyName("game_datetime")]
    public DateTime? GameDateTime { get; set; }

    [Column("stadium_name")]
    [JsonPropertyName("stadium_name")]
    public string? StadiumName { get; set; }

    [Column("stadium_style")]
    [JsonPropertyName("stadium_style")]
    public string? StadiumStyle { get; set; } 

    [Column("is_divisional_game")]
    [JsonPropertyName("is_divisional_game")]
    public bool? IsDivisionalGame { get; set; }

    [Column("home_rest_days")]
    [JsonPropertyName("home_rest_days")]
    public int? HomeRestDays { get; set; }

    [Column("away_rest_days")]
    [JsonPropertyName("away_rest_days")]
    public int? AwayRestDays { get; set; }

    // Betting odds
    [Column("home_moneyline")]
    [JsonPropertyName("home_moneyline")]
    public int? HomeMoneyline { get; set; }

    [Column("away_moneyline")]
    [JsonPropertyName("away_moneyline")]
    public int? AwayMoneyline { get; set; }

    [Column("home_spread_odds")]
    [JsonPropertyName("home_spread_odds")]
    public int? HomeSpreadOdds { get; set; }

    [Column("away_spread_odds")]
    [JsonPropertyName("away_spread_odds")]
    public int? AwaySpreadOdds { get; set; }

    [Column("spread_line")]
    [JsonPropertyName("spread_line")]
    public double? SpreadLine { get; set; }

    [Column("total_line")]
    [JsonPropertyName("total_line")]
    public double? TotalLine { get; set; }

    [Column("under_odds")]
    [JsonPropertyName("under_odds")]
    public int? UnderOdds { get; set; }

    [Column("over_odds")]
    [JsonPropertyName("over_odds")]
    public int? OverOdds { get; set; }
}
