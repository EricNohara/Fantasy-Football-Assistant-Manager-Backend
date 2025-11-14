using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

// relationship table mapping players to user's rosters
[Table("team_members")]
public class LeagueMember: BaseModel
{
    [PrimaryKey("league_id", false)]
    [Column("league_id")]
    [JsonPropertyName("league_id")]
    public String LeagueId { get; set; }

    [PrimaryKey("player_id", false)]
    [Column("player_id")]
    [JsonPropertyName("player_id")]
    public String PlayerId { get; set; }

    [Column("picked")]
    [JsonPropertyName("picked")]
    public Boolean Picked { get; set; }

}
