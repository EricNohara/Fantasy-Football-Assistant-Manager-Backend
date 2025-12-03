using System.Text.Json.Serialization;

namespace FFOracle.DTOs
{
    //contains the type of id of a league member; team or player
    public class MemberIdType
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
    }
}
