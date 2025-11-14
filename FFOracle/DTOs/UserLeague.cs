using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

public class UserLeague
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("scoring_settings_id")]
    public string ScoringSettingsId { get; set; } = string.Empty;

    [JsonPropertyName("roster_settings_id")]
    public string RosterSettingsId { get; set; } = string.Empty;
}
