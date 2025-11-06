using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Services;
using Fantasy_Football_Assistant_Manager.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerWeeklyStatController : ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;
    private readonly UpdateSupabaseService _updateSupabaseService;

    // injects NflVerseService via dependency injection
    public PlayerWeeklyStatController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
        _updateSupabaseService = new UpdateSupabaseService(_nflVerseService, _supabase);
    }

    // POST route for adding all weekly player stats up to the most recent week
    // to be called from Azure Functions App on a schedule for weekly updates
    [HttpPost("all")]
    public async Task<IActionResult> PostAll()
    {
        try
        {
            var (startWeek, endWeek) = await _updateSupabaseService.UpdateStatsFromLastThreeWeeksAsync();
            return Ok(new { message = $"Inserted weekly stats successfully for weeks {startWeek}–{endWeek}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error inserting weekly stats: {ex.Message}");
        }
    }

    // DELETE route for deleting all weekly player stats
    // used for testing
    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            bool moreRecordsToDelete = true;
            var batchSize = 500;

            // supabase cannot handle deleting thousands or records at once, so break up into batches
            while (moreRecordsToDelete)
            {
                // get all PlayerStats to delete from player_stats table
                var playerStatsResponse = await _supabase
                    .From<WeeklyPlayerStat>()
                    .Select("player_stats_id")
                    .Limit(batchSize)
                    .Get();

                var batchIds = playerStatsResponse.Models
                    .Select(s => s.PlayerStatsId)
                    .ToList();

                // break if deleted all records
                if (!batchIds.Any())
                {
                    moreRecordsToDelete = false;
                    break;
                }

                // delete all batch records from player_stats table
                await _supabase
                    .From<PlayerStat>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.In, batchIds)
                    .Delete();

                // delete all batch records from weekly_player_stats table
                await _supabase
                    .From<WeeklyPlayerStat>()
                    .Filter("player_stats_id", Supabase.Postgrest.Constants.Operator.In, batchIds)
                    .Delete();
            }

            return Ok(new { message = "Successfully deleted all weekly stats" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting weekly stats: {ex.Message}");
        }
    }
}
