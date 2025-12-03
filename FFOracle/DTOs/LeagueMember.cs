using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//A player's information relevant to being in a league
public class LeagueMember
{
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [JsonPropertyName("player_id")]
    public String PlayerId { get; set; }

    [JsonPropertyName("picked")]
    public Boolean Picked { get; set; } = false;
}
