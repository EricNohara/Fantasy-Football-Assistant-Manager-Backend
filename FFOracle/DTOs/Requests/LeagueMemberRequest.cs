using Newtonsoft.Json;

namespace FFOracle.DTOs.Requests;

//Data to identify a league member
public class LeagueMemberRequest
{
    [JsonProperty("league_id")]
    public Guid LeagueId { get; set; }

    [JsonProperty("member_id")]
    public string MemberId { get; set; }

    [JsonProperty("is_defense")]
    public bool IsDefense { get; set; }
}

//Data to request swapping two members
public class SwapLeagueMemberRequest
{
    [JsonProperty("league_id")]
    public Guid LeagueId { get; set; }

    [JsonProperty("old_member_id")]
    public string OldMemberId { get; set; }

    [JsonProperty("old_is_defense")]
    public bool OldIsDefense { get; set; }

    [JsonPropertyAttribute("new_member_id")]
    public string NewMemberId { get; set; }

    [JsonProperty("new_is_defense")]
    public bool NewIsDefense { get; set; }
}
