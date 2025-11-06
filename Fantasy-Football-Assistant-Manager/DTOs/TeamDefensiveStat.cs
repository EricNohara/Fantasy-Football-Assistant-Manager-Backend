using System;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs;

public class TeamDefensiveStat
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("tackles_for_loss")]
    public int? TacklesForLoss { get; set; }

    [JsonPropertyName("tackles_for_loss_yards")]
    public int? TacklesForLossYards { get; set; }

    [JsonPropertyName("fumbles_for")]
    public int? FumblesFor { get; set; }

    [JsonPropertyName("sacks_for")]
    public int? SacksFor { get; set; }

    [JsonPropertyName("sack_yards_for")]
    public int? SackYardsFor { get; set; }

    [JsonPropertyName("interceptions_for")]
    public int? InterceptionsFor { get; set; }

    [JsonPropertyName("interception_yards_for")]
    public int? InterceptionYardsFor { get; set; }

    [JsonPropertyName("def_tds")]
    public int? DefTds { get; set; }

    [JsonPropertyName("safeties")]
    public int? Safeties { get; set; }

    [JsonPropertyName("pass_defended")]
    public int? PassDefended { get; set; }
}
