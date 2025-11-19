using Newtonsoft.Json;

namespace FFOracle.DTOs.Requests;

public class AddLeagueMemberRequest
{
    [JsonProperty("league_id")]
    public Guid LeagueId { get; set; }

    [JsonProperty("member_id")]
    public string MemberId { get; set; }

    [JsonProperty("is_defense")]
    public bool IsDefense { get; set; }
}
