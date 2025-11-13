using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

// stores a player's stats for a single week    
[Table("weekly_player_stats")]
public class WeeklyPlayerStat: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("week")]
    [JsonPropertyName("week")]
    public int Week { get; set; }

    [Column("season_start_year")]
    public int SeasonStartYear { get; set; }

    [Column("player_id")]
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; } = string.Empty;

    [Column("player_stats_id")]
    [JsonPropertyName("player_stats_id")]
    public Guid PlayerStatsId { get; set; }
}
