using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//all information on a scoring setting entry
public class ScoringSetting
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

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
