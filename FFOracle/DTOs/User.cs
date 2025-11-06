using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

public class User
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("roster_settings_id")]
    public Guid? RosterSettingsId { get; set; }

    [JsonPropertyName("scoring_settings_id")]
    public Guid? ScoringSettingsId { get; set; }

    [JsonPropertyName("tokens_left")]
    public int TokensLeft { get; set; } = 0;
}
