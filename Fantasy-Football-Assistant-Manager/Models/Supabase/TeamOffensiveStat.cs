using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models.Supabase;

// stats used when finding DEF to start by matchup
[Table("team_offensive_stats")]

public class TeamOffensiveStat: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("completions")]
    [JsonPropertyName("completions")]
    public int? Completions { get; set; }

    [Column("attempts")]
    [JsonPropertyName("attempts")]
    public int? Attempts { get; set; }

    [Column("passing_yards")]
    [JsonPropertyName("passing_yards")]
    public int? PassingYards { get; set; }

    [Column("passing_tds")]
    [JsonPropertyName("passing_tds")]
    public int? PassingTds { get; set; }

    [Column("passing_interceptions")]
    [JsonPropertyName("passing_interceptions")]
    public int? PassingInterceptions { get; set; }

    [Column("sacks_against")]
    [JsonPropertyName("sacks_against")]
    public int? SacksAgainst { get; set; }

    [Column("fumbles_against")]
    [JsonPropertyName("fumbles_against")]
    public int? FumblesAgainst { get; set; }

    [Column("carries")]
    [JsonPropertyName("carries")]
    public int? Carries { get; set; }

    [Column("rushing_yards")]
    [JsonPropertyName("rushing_yards")]
    public int? RushingYards { get; set; }

    [Column("rushing_tds")]
    [JsonPropertyName("rushing_tds")]
    public int? RushingTds { get; set; }

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
}
