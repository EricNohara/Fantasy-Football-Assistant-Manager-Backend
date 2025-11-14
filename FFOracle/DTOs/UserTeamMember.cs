using System.Text.Json.Serialization;

namespace FFOracle.DTOs
{
    public class UserTeamMember
    {
        [JsonPropertyName("picked")]
        public bool Picked { get; set; } = false;

        public required DTOs.PlayerWithStats Player { get; set; }
    }
}
