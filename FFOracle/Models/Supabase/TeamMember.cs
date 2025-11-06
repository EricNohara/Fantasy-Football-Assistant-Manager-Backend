using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

// relationship table mapping players to user's rosters
public class TeamMember: BaseModel
{
    [PrimaryKey("user_id", false)]
    [Column("user_id")]
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [PrimaryKey("player_id", false)]
    [Column("player_id")]
    [JsonPropertyName("player_id")]
    public Guid PlayerId { get; set; }
}
