using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models;
public class Team: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Column("offensive_stats_id")]
    [JsonPropertyName("offensive_stats_id")]
    public Guid? OffensiveStatsId { get; set; }

    [Column("defensive_stats_id")]
    [JsonPropertyName("defensive_stats_id")]
    public Guid? DefensiveStatsId { get; set; }
}
