using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("player_stats")]
public class PlayerStat : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("completions")]
    [JsonPropertyName("completions")]
    public int? Completions { get; set; }

    [Column("passing_attempts")]
    [JsonPropertyName("passing_attempts")]
    public int? PassingAttempts { get; set; }

    [Column("passing_yards")]
    [JsonPropertyName("passing_yards")]
    public int? PassingYards { get; set; }

    [Column("passing_tds")]
    [JsonPropertyName("passing_tds")]
    public int? PassingTds { get; set; }

    [Column("interceptions_against")]
    [JsonPropertyName("interceptions_against")]
    public int? InterceptionsAgainst { get; set; }

    [Column("sacks_against")]
    [JsonPropertyName("sacks_against")]
    public int? SacksAgainst { get; set; }

    [Column("fumbles_against")]
    [JsonPropertyName("fumbles_against")]
    public int? FumblesAgainst { get; set; }

    [Column("passing_first_downs")]
    [JsonPropertyName("passing_first_downs")]
    public int? PassingFirstDowns { get; set; }

    [Column("passing_epa")]
    [JsonPropertyName("passing_epa")]
    public float? PassingEpa { get; set; }

    [Column("carries")]
    [JsonPropertyName("carries")]
    public int? Carries { get; set; }

    [Column("rushing_yards")]
    [JsonPropertyName("rushing_yards")]
    public int? RushingYards { get; set; }

    [Column("rushing_tds")]
    [JsonPropertyName("rushing_tds")]
    public int? RushingTds { get; set; }

    [Column("rushing_first_downs")]
    [JsonPropertyName("rushing_first_downs")]
    public int? RushingFirstDowns { get; set; }

    [Column("rushing_epa")]
    [JsonPropertyName("rushing_epa")]
    public float? RushingEpa { get; set; }

    [Column("receptions")]
    [JsonPropertyName("receptions")]
    public int? Receptions { get; set; }

    [Column("targets")]
    [JsonPropertyName("targets")]
    public int? Targets { get; set; }

    [Column("receiving_yards")]
    [JsonPropertyName("receiving_yards")]
    public int? ReceivingYards { get; set; }

    [Column("receiving_tds")]
    [JsonPropertyName("receiving_tds")]
    public int? ReceivingTds { get; set; }

    [Column("receiving_first_downs")]
    [JsonPropertyName("receiving_first_downs")]
    public int? ReceivingFirstDowns { get; set; }

    [Column("receiving_epa")]
    [JsonPropertyName("receiving_epa")]
    public float? ReceivingEpa { get; set; }

    [Column("fg_made_list")]
    [JsonPropertyName("fg_made_list")]
    public List<int>? FgMadeList { get; set; }

    [Column("fg_missed_list")]
    [JsonPropertyName("fg_missed_list")]
    public List<int>? FgMissedList { get; set; }

    [Column("fg_blocked_list")]
    [JsonPropertyName("fg_blocked_list")]
    public List<int>? FgBlockedList { get; set; }

    [Column("pat_attempts")]
    [JsonPropertyName("pat_attempts")]
    public int? PadAttempts { get; set; }

    [Column("pat_percent")]
    [JsonPropertyName("pat_percent")]
    public float? PatPercent { get; set; }

    [Column("fantasy_points")]
    [JsonPropertyName("fantasy_points")]
    public float? FantasyPoints { get; set; }

    [Column("fantasy_points_ppr")]
    [JsonPropertyName("fantasy_points_ppr")]
    public float? FantasyPointsPpr { get; set; }
}
