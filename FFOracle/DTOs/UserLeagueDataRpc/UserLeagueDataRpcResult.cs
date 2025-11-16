using System.Text.Json.Serialization;

namespace FFOracle.DTOs.UserLeagueDataRpc;

public class UserLeagueDataRpcResult
{
    [JsonPropertyName("players")]
    public List<PlayerDto> Players { get; set; }

    [JsonPropertyName("defenses")]
    public List<DefenseDto> Defenses { get; set; }

    [JsonPropertyName("rosterSettings")]
    public RosterSettingsDto RosterSettings { get; set; }

    [JsonPropertyName("scoringSettings")]
    public ScoringSettingsDto ScoringSettings { get; set; }
}

public class PlayerDto
{
    [JsonPropertyName("player")]
    public PlayerInfoDto Player { get; set; }

    [JsonPropertyName("seasonStats")]
    public SeasonStatsDto SeasonStats { get; set; }

    [JsonPropertyName("weeklyStats")]
    public List<WeeklyStatsDto> WeeklyStats { get; set; }

    [JsonPropertyName("opponent")]
    public OpponentDto Opponent { get; set; }
}

public class OpponentDto
{
    [JsonPropertyName("team")]
    public OpponentTeamDto Team { get; set; }

    [JsonPropertyName("defensiveStats")]
    public DefenseSeasonStatsDto DefensiveStats { get; set; }
}

public class OpponentTeamDto
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
}

public class PlayerInfoDto
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("team_id")] public string TeamId { get; set; }
    [JsonPropertyName("position")] public string Position { get; set; }
    [JsonPropertyName("status_description")] public string StatusDescription { get; set; }
}

public class SeasonStatsDto
{
    [JsonPropertyName("carries")] public int? Carries { get; set; }
    [JsonPropertyName("targets")] public int? Targets { get; set; }
    [JsonPropertyName("receptions")] public int? Receptions { get; set; }
    [JsonPropertyName("completions")] public int? Completions { get; set; }
    [JsonPropertyName("passing_epa")] public double? PassingEpa { get; set; }
    [JsonPropertyName("passing_tds")] public int? PassingTds { get; set; }
    [JsonPropertyName("rushing_epa")] public double? RushingEpa { get; set; }
    [JsonPropertyName("rushing_tds")] public int? RushingTds { get; set; }
    [JsonPropertyName("passing_yards")] public int? PassingYards { get; set; }
    [JsonPropertyName("receiving_epa")] public double? ReceivingEpa { get; set; }
    [JsonPropertyName("receiving_tds")] public int? ReceivingTds { get; set; }
    [JsonPropertyName("rushing_yards")] public int? RushingYards { get; set; }
    [JsonPropertyName("sacks_against")] public int? SacksAgainst { get; set; }
    [JsonPropertyName("fantasy_points")] public double FantasyPoints { get; set; }
    [JsonPropertyName("fumbles_against")] public int? FumblesAgainst { get; set; }
    [JsonPropertyName("receiving_yards")] public int? ReceivingYards { get; set; }
    [JsonPropertyName("passing_attempts")] public int? PassingAttempts { get; set; }
    [JsonPropertyName("fantasy_points_ppr")] public double FantasyPointsPpr { get; set; }
    [JsonPropertyName("passing_first_downs")] public int? PassingFirstDowns { get; set; }
    [JsonPropertyName("rushing_first_downs")] public int? RushingFirstDowns { get; set; }
    [JsonPropertyName("interceptions_against")] public int? InterceptionsAgainst { get; set; }
    [JsonPropertyName("receiving_first_downs")] public int? ReceivingFirstDowns { get; set; }
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

public class WeeklyStatsDto
{
    [JsonPropertyName("week")] public int Week { get; set; }
    [JsonPropertyName("stats")] public WeeklyStatDetailDto Stats { get; set; }
}

public class WeeklyStatDetailDto
{
    [JsonPropertyName("carries")] public int? Carries { get; set; }
    [JsonPropertyName("targets")] public int? Targets { get; set; }
    [JsonPropertyName("receptions")] public int? Receptions { get; set; }
    [JsonPropertyName("completions")] public int? Completions { get; set; }
    [JsonPropertyName("passing_epa")] public double? PassingEpa { get; set; }
    [JsonPropertyName("passing_tds")] public int? PassingTds { get; set; }
    [JsonPropertyName("passing_yards")] public int? PassingYards { get; set; }
    [JsonPropertyName("receiving_epa")] public double? ReceivingEpa { get; set; }
    [JsonPropertyName("receiving_tds")] public int? ReceivingTds { get; set; }
    [JsonPropertyName("rushing_yards")] public int? RushingYards { get; set; }
    [JsonPropertyName("sacks_against")] public int? SacksAgainst { get; set; }
    [JsonPropertyName("fantasy_points")] public double FantasyPoints { get; set; }
    [JsonPropertyName("rushing_epa")] public double? RushingEpa { get; set; }
    [JsonPropertyName("rushing_tds")] public int? RushingTds { get; set; }
    [JsonPropertyName("fumbles_against")] public int? FumblesAgainst { get; set; }
    [JsonPropertyName("receiving_yards")] public int? ReceivingYards { get; set; }
    [JsonPropertyName("passing_attempts")] public int? PassingAttempts { get; set; }
    [JsonPropertyName("fantasy_points_ppr")] public double FantasyPointsPpr { get; set; }
    [JsonPropertyName("passing_first_downs")] public int? PassingFirstDowns { get; set; }
    [JsonPropertyName("rushing_first_downs")] public int? RushingFirstDowns { get; set; }
    [JsonPropertyName("interceptions_against")] public int? InterceptionsAgainst { get; set; }
    [JsonPropertyName("receiving_first_downs")] public int? ReceivingFirstDowns { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("pat_percent")] public double? PatPercent { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("pat_attempts")] public int? PatAttempts { get; set; }

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

public class RosterSettingsDto
{
    [JsonPropertyName("k_count")] public int KCount { get; set; }
    [JsonPropertyName("ir_count")] public int IrCount { get; set; }
    [JsonPropertyName("qb_count")] public int QbCount { get; set; }
    [JsonPropertyName("rb_count")] public int RbCount { get; set; }
    [JsonPropertyName("te_count")] public int TeCount { get; set; }
    [JsonPropertyName("wr_count")] public int WrCount { get; set; }
    [JsonPropertyName("def_count")] public int DefCount { get; set; }
    [JsonPropertyName("flex_count")] public int FlexCount { get; set; }
    [JsonPropertyName("bench_count")] public int BenchCount { get; set; }
}

public class ScoringSettingsDto
{
    [JsonPropertyName("points_per_td")] public double PointsPerTd { get; set; }
    [JsonPropertyName("points_per_reception")] public double PointsPerReception { get; set; }
    [JsonPropertyName("points_per_passing_yard")] public double PointsPerPassingYard { get; set; }
    [JsonPropertyName("points_per_rushing_yard")] public double PointsPerRushingYard { get; set; }
    [JsonPropertyName("points_per_reception_yard")] public double PointsPerReceptionYard { get; set; }
}

public class DefenseDto
{
    [JsonPropertyName("team")]
    public DefenseTeamInfoDto Team { get; set; }

    [JsonPropertyName("seasonStats")]
    public DefenseSeasonStatsDto SeasonStats { get; set; }
}

public class DefenseTeamInfoDto
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("conference")] public string Conference { get; set; }
    [JsonPropertyName("division")] public string Division { get; set; }
}

public class DefenseSeasonStatsDto
{
    [JsonPropertyName("tackles_for_loss")] public int? TacklesForLoss { get; set; }
    [JsonPropertyName("tackles_for_loss_yards")] public int? TacklesForLossYards { get; set; }
    [JsonPropertyName("fumbles_for")] public int? FumblesFor { get; set; }
    [JsonPropertyName("sacks_for")] public int? SacksFor { get; set; }
    [JsonPropertyName("sack_yards_for")] public int? SackYardsFor { get; set; }
    [JsonPropertyName("interceptions_for")] public int? InterceptionsFor { get; set; }
    [JsonPropertyName("interception_yards_for")] public int? InterceptionYardsFor { get; set; }
    [JsonPropertyName("def_tds")] public int? DefensiveTouchdowns { get; set; }
    [JsonPropertyName("safeties")] public int? Safeties { get; set; }
    [JsonPropertyName("pass_defended")] public int? PassDefended { get; set; }
}
