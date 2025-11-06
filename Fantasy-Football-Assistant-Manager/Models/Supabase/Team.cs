using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("teams")]
public class Team : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Column("conference")]
    [JsonPropertyName("conference")]
    public string Conference { get; set; } = string.Empty;

    [Column("division")]
    [JsonPropertyName("division")]
    public string Division { get; set; } = string.Empty;

    [Column("logo_url")]
    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;

    [Column("offensive_stats_id")]
    [JsonPropertyName("offensive_stats_id")]
    public Guid? OffensiveStatsId { get; set; }

    [Column("defensive_stats_id")]
    [JsonPropertyName("defensive_stats_id")]
    public Guid? DefensiveStatsId { get; set; }
}
