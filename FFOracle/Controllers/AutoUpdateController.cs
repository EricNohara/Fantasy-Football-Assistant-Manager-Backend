using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;


namespace FFOracle.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoUpdateController: ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;
    private readonly UpdateSupabaseService _updateSupabaseService;

    public AutoUpdateController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
        _updateSupabaseService = new UpdateSupabaseService(_nflVerseService, _supabase);
    }

    // PUT route for weekly updates
    [HttpPut("weekly")]
    public async Task<IActionResult> PutWeekly()
    {
        try
        {
            // update weekly player stats (add the new week)
            await _updateSupabaseService.UpdateStatsFromLastThreeWeeksAsync();

            // update the season player stats
            await _updateSupabaseService.UpdateAllPlayerSeasonStatsAsync();

            // update the non stat data for players (e.g. status)
            await _updateSupabaseService.UpdateAllPlayerNonStatDataAsync();

            // update the team season stats
            await _updateSupabaseService.UpdateAllTeamSeasonStatsAsync();

            // update the games this week (delete and add new games)
            await _updateSupabaseService.UpdateGamesThisWeekAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error performing the weekly update: {ex.Message}");
        }
    }

    // PUT route for daily updates (player injury status, game odds)
    [HttpPut("daily")]
    public async Task<IActionResult> PutDaily()
    {
        try
        {
            // update the non stat data for players (e.g. status)
            await _updateSupabaseService.UpdateAllPlayerNonStatDataAsync();

            // update the games this week (delete and add new games)
            await _updateSupabaseService.UpdateGamesThisWeekAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error performing the weekly update: {ex.Message}");
        }
    }
}
