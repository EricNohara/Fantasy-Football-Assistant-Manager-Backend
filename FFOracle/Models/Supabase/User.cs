using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("users")]
public class User: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("team_name")]
    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }

    [Column("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Column("roster_settings_id")]
    [JsonPropertyName("roster_settings_id")]
    public Guid? RosterSettingsId { get; set; }

    [Column("scoring_settings_id")]
    [JsonPropertyName("scoring_settings_id")]
    public Guid? ScoringSettingsId { get; set; }

    [Column("tokens_left")]
    [JsonPropertyName("tokens_left")]
    public int TokensLeft { get; set; } = 0;
}
