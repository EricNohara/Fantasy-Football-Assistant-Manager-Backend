using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

// A DTO to represent a player with all stats included.
public class PlayerWithStats
{
    [JsonPropertyName("player")]
    public required Player Player { get; set; }

    [JsonPropertyName("season_stats")]
    public required PlayerStat SeasonStat { get; set; }

    [JsonPropertyName("weekly_stats")]
    public List<PlayerStatWithWeekNum> WeeklyStats { get; set; } = new List<PlayerStatWithWeekNum>();

}
