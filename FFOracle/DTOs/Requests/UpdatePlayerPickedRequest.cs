using System.Text.Json.Serialization;

namespace FFOracle.DTOs.Requests;

public class UpdatePlayerPickedRequest
{
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [JsonPropertyName("member_id")]
    public string MemberId { get; set; }

    [JsonPropertyName("picked")]
    public bool Picked { get; set; }

    [JsonPropertyName("is_defense")]
    public bool IsDefense { get; set; }
}