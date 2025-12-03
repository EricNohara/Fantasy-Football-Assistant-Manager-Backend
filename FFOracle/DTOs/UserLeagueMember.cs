using System.Text.Json.Serialization;

namespace FFOracle.DTOs
{
    //information on a single player in a league
    public class UserLeagueMember
    {
        [JsonPropertyName("picked")]
        public bool Picked { get; set; } = false;

        public required DTOs.PlayerWithStats Player { get; set; }
    }
}
