using FFOracle.Models.Supabase;
using FFOracle.Services;
using FFOracle.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace FFOracle.Controllers.Internal;

public class TeamSeasonStatController : ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;
    private readonly UpdateSupabaseService _updateSupabaseService;

    public TeamSeasonStatController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
        _updateSupabaseService = new UpdateSupabaseService(_nflVerseService, _supabase);
    }

    [HttpPost("all")]
    public async Task<IActionResult> PostAll()
    {
        try
        {
            await _updateSupabaseService.UpdateAllTeamSeasonStatsAsync();
            return Ok(new { message = "Team stats inserted successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error populating team stats: {ex.Message}");
        }
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            // delete all offensive stats
            await _supabase
                .From<TeamOffensiveStat>()
                .Where(s => s.Id != null)
                .Delete();

            // delete all defensive stats
            await _supabase
                .From<TeamDefensiveStat>()
                .Where(s => s.Id != null)
                .Delete();

            // the foreign keys set to null automatically in teams table
            return Ok(new { message = "Successfully deleted all team stats" });
        } catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting all team stats: {ex.Message}");
        }
    }
}
