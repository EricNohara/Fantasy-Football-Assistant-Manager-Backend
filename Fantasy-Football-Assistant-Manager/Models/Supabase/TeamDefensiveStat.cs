using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

// used to store a team's defensive stats
[Table("team_defensive_stats")]

public class TeamDefensiveStat: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("tackles_for_loss")]
    [JsonPropertyName("tackles_for_loss")]
    public int? TacklesForLoss { get; set; }

    [Column("tackles_for_loss_yards")]
    [JsonPropertyName("tackles_for_loss_yards")]
    public int? TacklesForLossYards { get; set; }

    [Column("fumbles_for")]
    [JsonPropertyName("fumbles_for")]
    public int? FumblesFor { get; set; }

    [Column("sacks_for")]
    [JsonPropertyName("sacks_for")]
    public int? SacksFor { get; set; }

    [Column("sack_yards_for")]
    [JsonPropertyName("sack_yards_for")]
    public int? SackYardsFor { get; set; }

    [Column("interceptions_for")]
    [JsonPropertyName("interceptions_for")]
    public int? InterceptionsFor { get; set; }

    [Column("interception_yards_for")]
    [JsonPropertyName("interception_yards_for")]
    public int? InterceptionYardsFor { get; set; }

    [Column("def_tds")]
    [JsonPropertyName("def_tds")]
    public int? DefTds { get; set; }

    [Column("safeties")]
    [JsonPropertyName("safeties")]
    public int? Safeties { get; set; }

    [Column("pass_defended")]
    [JsonPropertyName("pass_defended")]
    public int? PassDefended { get; set; }
}
