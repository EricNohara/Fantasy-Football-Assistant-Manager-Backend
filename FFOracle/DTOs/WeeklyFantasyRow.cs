using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

public class WeeklyFantasyRow
{
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; }

    [JsonPropertyName("fantasy_points")]
    public double FantasyPoints { get; set; }
}