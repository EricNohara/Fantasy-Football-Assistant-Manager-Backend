using System;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs;

public class Team
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("offensive_stats_id")]
    public Guid? OffensiveStatsId { get; set; }

    [JsonPropertyName("defensive_stats_id")]
    public Guid? DefensiveStatsId { get; set; }
}
