using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models;

// stores a player's stats for a single week
public class WeeklyPlayerStat: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("week")]
    [JsonPropertyName("week")]
    public int Week { get; set; }

    [Column("player_id")]
    [JsonPropertyName("player_id")]
    public Guid PlayerId { get; set; }

    [Column("player_stats_id")]
    [JsonPropertyName("player_stats_id")]
    public Guid PlayerStatsId { get; set; }
}
