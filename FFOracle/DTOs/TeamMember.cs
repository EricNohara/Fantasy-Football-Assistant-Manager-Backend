using System;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

public class TeamMember
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; }

    [JsonPropertyName("picked")]
    public Boolean Picked { get; set; } = false;
}
