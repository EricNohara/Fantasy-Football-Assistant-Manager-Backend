using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

// relationship table mapping players to user's rosters
[Table("league_offensive_members")]
public class LeagueOffensiveMember: BaseModel
{
    [PrimaryKey("player_id", false)]
    [Column("player_id")]
    [JsonPropertyName("player_id")]
    public String PlayerId { get; set; }

    [PrimaryKey("league_id", false)]
    [Column("league_id")]
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [Column("picked")]
    [JsonPropertyName("picked")]
    public Boolean Picked { get; set; }
}

[Table("league_defensive_members")]
public class LeagueDefensiveMember : BaseModel
{
    [PrimaryKey("team_id", false)]
    [Column("team_id")]
    [JsonPropertyName("team_id")]
    public string TeamId { get; set; }

    [PrimaryKey("league_id", false)]
    [Column("league_id")]
    [JsonPropertyName("league_id")]
    public Guid LeagueId { get; set; }

    [Column("picked")]
    [JsonPropertyName("picked")]
    public Boolean Picked { get; set; }
}
