using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//information on a single league
public class UserLeague
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("scoring_settings_id")]
    public Guid ScoringSettingsId { get; set; }

    [JsonPropertyName("roster_settings_id")]
    public Guid RosterSettingsId { get; set; }
}
