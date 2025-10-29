using System;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs;

public class WeeklyPlayerStat
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("week")]
    public int Week { get; set; }

    [JsonPropertyName("player_id")]
    public String PlayerId { get; set; } = String.Empty;

    [JsonPropertyName("player_stats_id")]
    public Guid PlayerStatsId { get; set; }
}
