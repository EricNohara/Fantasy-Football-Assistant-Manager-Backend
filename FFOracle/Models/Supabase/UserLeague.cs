using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("user_leagues")]
public class UserLeague : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Column("user_id")]
    [JsonPropertyName("user_id")]
    public string UserID { get; set; } = string.Empty;

    [Column("scoring_settings_id")]
    [JsonPropertyName("scoring_settings_id")]
    public string ScoringSettingsID { get; set; } = string.Empty;

    [Column("roster_settings_id")]
    [JsonPropertyName("roster_settings_id")]
    public string RosterSettingsID { get; set; } = string.Empty;
}
