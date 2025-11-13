using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

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

    [Column("status")]
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [Column("status_description")]
    [JsonPropertyName("status_description")]
    public string? StatusDescription { get; set; }

    [Column("team_id")]
    [JsonPropertyName("team_id")]
    public string? TeamId { get; set; }

    [Column("season_stats_id")]
    [JsonPropertyName("season_stats_id")]
    public Guid? SeasonStatsId { get; set; }
}
