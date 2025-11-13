using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs
{
    public class UserTeamMember
    {
        [JsonPropertyName("picked")]
        public bool Picked { get; set; } = false;

        public required DTOs.PlayerWithStats Player { get; set; }
    }
}
