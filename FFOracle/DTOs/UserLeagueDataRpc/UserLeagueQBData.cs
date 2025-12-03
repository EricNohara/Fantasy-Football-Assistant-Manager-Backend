namespace FFOracle.DTOs.UserLeagueDataRpc;

using System.Text.Json.Serialization;

//A QB league member's data
public class UserLeagueQBData : IPlayerData
{
    [JsonPropertyName("player")]
    public PlayerInfoDto Player { get; set; }

    [JsonPropertyName("seasonStats")]
    public QBStatsDto SeasonStats { get; set; }

    [JsonPropertyName("weeklyStats")]
    public List<QBWeeklyStatsDto> WeeklyStats { get; set; }

    [JsonPropertyName("opponent")]
    public OpponentDto Opponent { get; set; }

    [JsonPropertyName("game")]
    public GameDto Game { get; set; }
}

//QB-relevant stats
public class QBStatsDto
{
    [JsonPropertyName("passing_attempts")] public int? PassingAttempts { get; set; }
    [JsonPropertyName("completions")] public int? Completions { get; set; }
    [JsonPropertyName("passing_epa")] public double? PassingEpa { get; set; }
    [JsonPropertyName("passing_tds")] public int? PassingTds { get; set; }
    [JsonPropertyName("passing_yards")] public int? PassingYards { get; set; }
    [JsonPropertyName("passing_first_downs")] public int? PassingFirstDowns { get; set; }
    [JsonPropertyName("sacks_against")] public int? SacksAgainst { get; set; }
    [JsonPropertyName("interceptions_against")] public int? InterceptionsAgainst { get; set; }
    [JsonPropertyName("fumbles_against")] public int? FumblesAgainst { get; set; }
    [JsonPropertyName("carries")] public int? Carries { get; set; }
    [JsonPropertyName("rushing_epa")] public double? RushingEpa { get; set; }
    [JsonPropertyName("rushing_tds")] public int? RushingTds { get; set; }
    [JsonPropertyName("rushing_first_downs")] public int? RushingFirstDowns { get; set; }
    [JsonPropertyName("rushing_yards")] public int? RushingYards { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("targets")] public int? Targets { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receptions")] public int? Receptions { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_epa")] public double? ReceivingEpa { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_yards")] public int? ReceivingYards { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_tds")] public int? ReceivingTds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_first_downs")] public int? ReceivingFirstDowns { get; set; }
    [JsonPropertyName("fantasy_points")] public double FantasyPoints { get; set; }
    [JsonPropertyName("fantasy_points_ppr")] public double FantasyPointsPpr { get; set; }
}

//QB-relevant weekly stats
public class QBWeeklyStatsDto
{
    [JsonPropertyName("week")] public int Week { get; set; }
    [JsonPropertyName("stats")] public QBStatsDto Stats { get; set; }
}