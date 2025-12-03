using FFOracle.DTOs.Responses;
using FFOracle.Models.Supabase;
using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using System.Text.Json;

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class LeaguePerformanceController: ControllerBase
{
    private readonly Client _supabase;
    private readonly SupabaseAuthService _authService;
    public LeaguePerformanceController(Client supabase, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _authService = authService;
    }

    // Read-only endpoint to fetch league performance data for a user
    [HttpGet("{leagueId:Guid}/week/{week:int}")]
    public async Task<IActionResult> GetLeaguePerformance(Guid leagueId, int week)
    {
        try
        {
            // Authorize user
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == Guid.Empty)
                return Unauthorized("Invalid token");

            // Verify league belongs to user
            var league = await _supabase
                .From<UserLeague>()
                .Where(l => l.Id == leagueId && l.UserID == userId)
                .Single();

            if (league == null)
                return Forbid("You do not have access to this league");

            // Load ALL weekly league performance rows
            var allLeaguePerf = await _supabase
                .From<WeeklyLeaguePerformance>()
                .Where(w => w.LeagueId == leagueId)
                .Order(w => w.Week, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();
            
            //Filter only the relevant columns
            var leaguePerformance = allLeaguePerf.Models
                .Select(lp => new WeeklyLeaguePerformanceDto
                {
                    LeagueId = lp.LeagueId,
                    Week = lp.Week,
                    ActualFpts = lp.ActualFpts,
                    MaxFpts = lp.MaxFpts,
                    Accuracy = lp.Accuracy
                })
                .ToList();

            // Load player performance ONLY for the selected week
            var weekPlayerPerf = await _supabase
                .From<WeeklyLeaguePlayerPerformance>()
                .Where(w => w.LeagueId == leagueId && w.Week == week)
                .Order(x => x.OverallRank, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();
            //filter relevant columns
            var playerPerformance = weekPlayerPerf.Models
                .Select(pp => new WeeklyLeaguePlayerPerformanceDto
                {
                    LeagueId = pp.LeagueId,
                    Week = pp.Week,
                    PlayerId = pp.PlayerId,
                    ActualFpts = pp.ActualFpts,
                    Picked = pp.Picked,
                    PositionRank = pp.PositionRank,
                    OverallRank = pp.OverallRank
                })
                .ToList();

            // Construct final response
            var response = new LeaguePerformanceResponse
            {
                LeaguePerformance = leaguePerformance,
                PlayerPerformance = playerPerformance
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Error fetching data from Supabase",
                details = ex.Message
            });
        }
    }
}
