using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("weekly_league_performance")]
public class WeeklyLeaguePerformance : BaseModel
{

    [PrimaryKey("league_id")]
    [Column("league_id")]
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [PrimaryKey("week")]
    [Column("week")]
    [JsonPropertyName("week")]
    public int Week { get; set; }

    [Column("actual_fpts")]
    [JsonPropertyName("actual_fpts")]
    public double ActualFpts { get; set; }

    [Column("max_fpts")]
    [JsonPropertyName("max_fpts")]
    public double MaxFpts { get; set; }

    [Column("accuracy")]
    [JsonPropertyName("accuracy")]
    public double Accuracy { get; set; }
}
