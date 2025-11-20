using System.Text.Json.Serialization;

namespace FFOracle.DTOs
{
    public class MemberIdType
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
    }
}
