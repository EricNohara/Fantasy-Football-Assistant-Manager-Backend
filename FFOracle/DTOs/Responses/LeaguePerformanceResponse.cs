using FFOracle.Models.Supabase;

namespace FFOracle.DTOs.Responses;

public class LeaguePerformanceResponse
{
    public List<WeeklyLeaguePerformanceDto> LeaguePerformance { get; set; }
    public List<WeeklyLeaguePlayerPerformanceDto> PlayerPerformance { get; set; }
}

public class WeeklyLeaguePerformanceDto
{
    public Guid LeagueId { get; set; }
    public int Week { get; set; }
    public double ActualFpts { get; set; }
    public double MaxFpts { get; set; }
    public double Accuracy { get; set; }
}

public class WeeklyLeaguePlayerPerformanceDto
{
    public Guid LeagueId { get; set; }
    public int Week { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public double ActualFpts { get; set; }
    public bool Picked { get; set; }
    public int PositionRank { get; set; }
    public int OverallRank { get; set; }
}
