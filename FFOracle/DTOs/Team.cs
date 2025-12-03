using System;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//all information on a single team
public class Team
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;


    [JsonPropertyName("conference")]
    public string Conference { get; set; } = string.Empty;

    [JsonPropertyName("division")]
    public string Division { get; set; } = string.Empty;

    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;

    [JsonPropertyName("offensive_stats_id")]
    public Guid? OffensiveStatsId { get; set; }

    [JsonPropertyName("defensive_stats_id")]
    public Guid? DefensiveStatsId { get; set; }
}
