using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models.Supabase;

[Table("players")]
public class Player: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Column("headshot_url")]
    [JsonPropertyName("headshot_url")]
    public string? HeadshotUrl { get; set; }

    [Column("position")]
    [JsonPropertyName("position")]
    public string Position { get; set; } = string.Empty;

    [Column("injury_status")]
    [JsonPropertyName("injury_status")]
    public string? InjuryStatus { get; set; }

    [Column("team_id")]
    [JsonPropertyName("team_id")]
    public Guid? TeamId { get; set; }

    [Column("season_stats_id")]
    [JsonPropertyName("season_stats_id")]
    public Guid? SeasonStatsId { get; set; }
}
