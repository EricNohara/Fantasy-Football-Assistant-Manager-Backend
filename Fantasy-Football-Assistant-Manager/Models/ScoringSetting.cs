using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models;

// model for league scoring settings
[Table("scoring_settings")]
public class ScoringSetting: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("points_per_td")]
    [JsonPropertyName("points_per_td")]
    public float PointsPerTd { get; set; }

    [Column("points_per_reception")]
    [JsonPropertyName("points_per_reception")]
    public float PointsPerReception { get; set; }

    [Column("points_per_passing_yard")]
    [JsonPropertyName("points_per_passing_yard")]
    public float PointsPerPassingYard { get; set; }

    [Column("points_per_rushing_yard")]
    [JsonPropertyName("points_per_rushing_yard")]
    public float PointsPerRushingYard { get; set; }

    [Column("points_per_reception_yard")]
    [JsonPropertyName("points_per_reception_yard")]
    public float PointsPerReceptionYard { get; set; }
}
