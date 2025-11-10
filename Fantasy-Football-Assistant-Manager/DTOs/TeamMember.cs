using System;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs;

public class TeamMember
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("player_id")]
    public Guid PlayerId { get; set; }

    [JsonPropertyName("picked")]
    public Boolean Picked { get; set; } = false;
}
