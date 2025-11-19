using System.Text.Json.Serialization;

namespace FFOracle.DTOs.Requests;

public class UpdateScoringSettingsRequest
{
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [JsonPropertyName("points_per_td")]
    public float PointsPerTd { get; set; }

    [JsonPropertyName("points_per_reception")]
    public float PointsPerReception { get; set; }

    [JsonPropertyName("points_per_passing_yard")]
    public float PointsPerPassingYard { get; set; }

    [JsonPropertyName("points_per_rushing_yard")]
    public float PointsPerRushingYard { get; set; }

    [JsonPropertyName("points_per_reception_yard")]
    public float PointsPerReceptionYard { get; set; }
}
