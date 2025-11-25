using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("weekly_league_player_performance")]
public class WeeklyLeaguePlayerPerformance: BaseModel
{

    [PrimaryKey("league_id")]
    [Column("league_id")]
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [PrimaryKey("week")]
    [Column("week")]
    [JsonPropertyName("week")]
    public int Week { get; set; }

    [PrimaryKey("player_id")]
    [Column("player_id")]
    [JsonPropertyName("player_id")]
    public String PlayerId { get; set; }

    [Column("actual_fpts")]
    [JsonPropertyName("actual_fpts")]
    public double ActualFpts { get; set; }

    [Column("max_fpts")]
    [JsonPropertyName("max_fpts")]
    public double MaxFpts { get; set; }

    [Column("was_correct")]
    [JsonPropertyName("was_correct")]
    public bool WasCorrect { get; set; }
}
