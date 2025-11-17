namespace FFOracle.DTOs.UserLeagueDataRpc;

using System.Text.Json.Serialization;

public class UserLeagueFlexData
{
    [JsonPropertyName("player")]
    public PlayerInfoDto Player { get; set; }

    [JsonPropertyName("seasonStats")]
    public FlexStatsDto SeasonStats { get; set; }

    [JsonPropertyName("weeklyStats")]
    public List<FlexWeeklyStatsDto> WeeklyStats { get; set; }

    [JsonPropertyName("opponent")]
    public OpponentDto Opponent { get; set; }

    [JsonPropertyName("game")]
    public GameDto Game { get; set; }
}

public class FlexStatsDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_attempts")] public int? PassingAttempts { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("completions")] public int? Completions { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_epa")] public double? PassingEpa { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_tds")] public int? PassingTds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_yards")] public int? PassingYards { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_first_downs")] public int? PassingFirstDowns { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("sacks_against")] public int? SacksAgainst { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("interceptions_against")] public int? InterceptionsAgainst { get; set; }
    [JsonPropertyName("fumbles_against")] public int? FumblesAgainst { get; set; }
    [JsonPropertyName("carries")] public int? Carries { get; set; }
    [JsonPropertyName("rushing_epa")] public double? RushingEpa { get; set; }
    [JsonPropertyName("rushing_tds")] public int? RushingTds { get; set; }
    [JsonPropertyName("rushing_first_downs")] public int? RushingFirstDowns { get; set; }
    [JsonPropertyName("rushing_yards")] public int? RushingYards { get; set; }
    [JsonPropertyName("targets")] public int? Targets { get; set; }
    [JsonPropertyName("receptions")] public int? Receptions { get; set; }
    [JsonPropertyName("receiving_epa")] public double? ReceivingEpa { get; set; }
    [JsonPropertyName("receiving_yards")] public int? ReceivingYards { get; set; }
    [JsonPropertyName("receiving_tds")] public int? ReceivingTds { get; set; }
    [JsonPropertyName("receiving_first_downs")] public int? ReceivingFirstDowns { get; set; }
    [JsonPropertyName("fantasy_points")] public double FantasyPoints { get; set; }
    [JsonPropertyName("fantasy_points_ppr")] public double FantasyPointsPpr { get; set; }
}

public class FlexWeeklyStatsDto
{
    [JsonPropertyName("week")] public int Week { get; set; }
    [JsonPropertyName("stats")] public FlexStatsDto Stats { get; set; }
}