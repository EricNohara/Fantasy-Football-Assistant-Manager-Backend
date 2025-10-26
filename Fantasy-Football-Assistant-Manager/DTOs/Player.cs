using System;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs;

public class Player
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("headshot_url")]
    public string? HeadshotUrl { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; } = string.Empty;

    [JsonPropertyName("injury_status")]
    public string? InjuryStatus { get; set; }

    [JsonPropertyName("team_id")]
    public Guid? TeamId { get; set; }

    [JsonPropertyName("season_stats_id")]
    public Guid? SeasonStatsId { get; set; }
}
