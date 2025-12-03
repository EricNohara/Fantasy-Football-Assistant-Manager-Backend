using FFOracle.Models.Supabase;

namespace FFOracle.DTOs.Responses;

//Information on a league performance across multiple weeks
public class LeaguePerformanceResponse
{
    public List<WeeklyLeaguePerformanceDto> LeaguePerformance { get; set; }
    public List<WeeklyLeaguePlayerPerformanceDto> PlayerPerformance { get; set; }
}

//League performance data for one week
public class WeeklyLeaguePerformanceDto
{
    public Guid LeagueId { get; set; }
    public int Week { get; set; }
    public double ActualFpts { get; set; }
    public double MaxFpts { get; set; }
    public double Accuracy { get; set; }
}

//Player performance data for one week
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
