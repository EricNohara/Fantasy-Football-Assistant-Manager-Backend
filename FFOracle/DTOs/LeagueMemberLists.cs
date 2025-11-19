using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//a DTO to hold lists of player ids and team ids for use in updating the league_member rows
// for both
public class LeagueMemberLists
{
    //Lists of offense player ids
    [JsonPropertyName("offense")]
    public List<String> Offense { get; set; } = new List<String>();

    //A list of defense team ids
    [JsonPropertyName("defense")]
    public List<String> Defense { get; set; } = new List<String>();
}
