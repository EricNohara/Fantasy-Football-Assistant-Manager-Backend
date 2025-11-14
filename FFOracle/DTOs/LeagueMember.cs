using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

public class LeagueMember
{
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [JsonPropertyName("player_id")]
    public String PlayerId { get; set; }

    [JsonPropertyName("picked")]
    public Boolean Picked { get; set; } = false;
}
