namespace FFOracle.DTOs.UserLeagueDataRpc;

using System.Text.Json.Serialization;

public class UserLeagueKData
{
    [JsonPropertyName("player")]
    public PlayerInfoDto Player { get; set; }

    [JsonPropertyName("seasonStats")]
    public KStatsDto SeasonStats { get; set; }

    [JsonPropertyName("weeklyStats")]
    public List<KWeeklyStatsDto> WeeklyStats { get; set; }

    [JsonPropertyName("opponent")]
    public OpponentDto Opponent { get; set; }
}

public class KStatsDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_attempts")] public int? PassingAttempts { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("completions")] public int? Completions { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_epa")] public double? PassingEpa { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_tds")] public int? PassingTds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_yards")] public int? PassingYards { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("passing_first_downs")] public int? PassingFirstDowns { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("sacks_against")] public int? SacksAgainst { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("interceptions_against")] public int? InterceptionsAgainst { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("fumbles_against")] public int? FumblesAgainst { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("carries")] public int? Carries { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("rushing_epa")] public double? RushingEpa { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("rushing_tds")] public int? RushingTds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("rushing_first_downs")] public int? RushingFirstDowns { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("rushing_yards")] public int? RushingYards { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("targets")] public int? Targets { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receptions")] public int? Receptions { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_epa")] public double? ReceivingEpa { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_yards")] public int? ReceivingYards { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_tds")] public int? ReceivingTds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("receiving_first_downs")] public int? ReceivingFirstDowns { get; set; }
    [JsonPropertyName("fantasy_points")] public double FantasyPoints { get; set; }
    [JsonPropertyName("fantasy_points_ppr")] public double FantasyPointsPpr { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("pat_attempts")] public int? PatAttempts { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("pat_percent")] public double? PatPercent { get; set; }

    private List<int> _fgMadeList;
    private List<int> _fgMissedList;
    private List<int> _fgBlockedList;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("fg_made_list")]
    public List<int> FgMadeList
    {
        get => _fgMadeList != null && _fgMadeList.Count > 0 ? _fgMadeList : null;
        set => _fgMadeList = value;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("fg_missed_list")]
    public List<int> FgMissedList
    {
        get => _fgMissedList != null && _fgMissedList.Count > 0 ? _fgMissedList : null;
        set => _fgMissedList = value;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("fg_blocked_list")]
    public List<int> FgBlockedList
    {
        get => _fgBlockedList != null && _fgBlockedList.Count > 0 ? _fgBlockedList : null;
        set => _fgBlockedList = value;
    }
}

public class KWeeklyStatsDto
{
    [JsonPropertyName("week")] public int Week { get; set; }
    [JsonPropertyName("stats")] public KStatsDto Stats { get; set; }
}