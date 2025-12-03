using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//information on a single player and their fantasy points
public class WeeklyFantasyRow
{
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; }

    [JsonPropertyName("fantasy_points")]
    public double FantasyPoints { get; set; }
}