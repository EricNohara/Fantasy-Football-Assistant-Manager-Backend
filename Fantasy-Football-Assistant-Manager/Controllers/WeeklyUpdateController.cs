using Fantasy_Football_Assistant_Manager.Services;
using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeeklyUpdateController: ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;
    private readonly UpdateSupabaseService _updateSupabaseService;

    public WeeklyUpdateController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
        _updateSupabaseService = new UpdateSupabaseService(_nflVerseService, _supabase);
    }

    // PUT route which updates all relevant databases
    [HttpPut]
    public async Task<IActionResult> Put()
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
}
