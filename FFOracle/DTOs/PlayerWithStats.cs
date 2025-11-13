using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs;

// A DTO to represent a player with all stats included.
public class PlayerWithStats
{
    [JsonPropertyName("player")]
    public required DTOs.Player Player { get; set; }

    [JsonPropertyName("season_stats")]
    public required DTOs.PlayerStat SeasonStat { get; set; }

    [JsonPropertyName("weekly_stats")]
    public List<DTOs.PlayerStatWithWeekNum> WeeklyStats { get; set; } = new List<DTOs.PlayerStatWithWeekNum>();

}
