using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//Full player stats for a single player
public class PlayerStat
{
    [JsonPropertyName("completions")]
    public int? Completions { get; set; }

    [JsonPropertyName("passing_attempts")]
    public int? PassingAttempts { get; set; }

    [JsonPropertyName("passing_yards")]
    public int? PassingYards { get; set; }

    [JsonPropertyName("passing_tds")]
    public int? PassingTds { get; set; }

    [JsonPropertyName("interceptions_against")]
    public int? InterceptionsAgainst { get; set; }

    [JsonPropertyName("sacks_against")]
    public int? SacksAgainst { get; set; }

    [JsonPropertyName("fumbles_against")]
    public int? FumblesAgainst { get; set; }

    [JsonPropertyName("passing_first_downs")]
    public int? PassingFirstDowns { get; set; }

    [JsonPropertyName("passing_epa")]
    public float? PassingEpa { get; set; }

    [JsonPropertyName("carries")]
    public int? Carries { get; set; }

    [JsonPropertyName("rushing_yards")]
    public int? RushingYards { get; set; }

    [JsonPropertyName("rushing_tds")]
    public int? RushingTds { get; set; }

    [JsonPropertyName("rushing_first_downs")]
    public int? RushingFirstDowns { get; set; }

    [JsonPropertyName("rushing_epa")]
    public float? RushingEpa { get; set; }

    [JsonPropertyName("receptions")]
    public int? Receptions { get; set; }

    [JsonPropertyName("targets")]
    public int? Targets { get; set; }

    [JsonPropertyName("receiving_yards")]
    public int? ReceivingYards { get; set; }

    [JsonPropertyName("receiving_tds")]
    public int? ReceivingTds { get; set; }

    [JsonPropertyName("receiving_first_downs")]
    public int? ReceivingFirstDowns { get; set; }

    [JsonPropertyName("receiving_epa")]
    public float? ReceivingEpa { get; set; }

    [JsonPropertyName("fg_made_list")]
    public List<int>? FgMadeList { get; set; }

    [JsonPropertyName("fg_missed_list")]
    public List<int>? FgMissedList { get; set; }

    [JsonPropertyName("fg_blocked_list")]
    public List<int>? FgBlockedList { get; set; }

    [JsonPropertyName("pat_attempts")]
    public int? PatAttempts { get; set; }

    [JsonPropertyName("pat_percent")]
    public float? PatPercent { get; set; }

    [JsonPropertyName("fantasy_points")]
    public float? FantasyPoints { get; set; }

    [JsonPropertyName("fantasy_points_ppr")]
    public float? FantasyPointsPpr { get; set; }
}
