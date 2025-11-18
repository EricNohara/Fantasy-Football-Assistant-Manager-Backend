using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//a DTO to hold information sent when updating a user's league.
public class LeagueUpdate
{
    //league ID, used to identify the league
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    //Settings DTOs; the league's associated settings will be set to match these
    [JsonPropertyName("RosterSetting")]
    public DTOs.RosterSetting RosterSetting { get; set; }
    [JsonPropertyName("ScoringSetting")]
    public DTOs.ScoringSetting ScoringSetting { get; set; }

    //Lists of offense player ids to indicate who is being picked, added, or removed
    [JsonPropertyName("PickedOffense")]
    public List<String> PickedOffense { get; set; } = new List<String>();
    [JsonPropertyName("AddedOffense")]
    public List<String> AddedOffense { get; set; } = new List<String>();
    [JsonPropertyName("RemovedOffense")]
    public List<String> RemovedOffense { get; set; } = new List<String>();

    //A list of defense team ids to indicate which is being picked, added, or removed
    [JsonPropertyName("PickedDefense")]
    public List<String> PickedDefense { get; set; } = new List<String>();
    [JsonPropertyName("AddedDefense")]
    public List<String> AddedDefense { get; set; } = new List<String>();
    [JsonPropertyName("RemovedDefense")]
    public List<String> RemovedDefense { get; set; } = new List<String>();
}
