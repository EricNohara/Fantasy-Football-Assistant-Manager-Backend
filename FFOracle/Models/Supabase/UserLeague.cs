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
    public Guid Id { get; set; }

    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Column("user_id")]
    [JsonPropertyName("user_id")]
    public Guid UserID { get; set; }

    [Column("scoring_settings_id")]
    [JsonPropertyName("scoring_settings_id")]
    public Guid ScoringSettingsID { get; set; }

    [Column("roster_settings_id")]
    [JsonPropertyName("roster_settings_id")]
    public Guid RosterSettingsID { get; set; }
}
