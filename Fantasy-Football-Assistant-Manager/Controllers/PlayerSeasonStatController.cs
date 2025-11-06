using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Services;
using Fantasy_Football_Assistant_Manager.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerSeasonStatController: ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;

    // injects NflVerseService via dependency injection
    public PlayerSeasonStatController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    // DELETE route for testing
    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            // Delete all records from the players table
            await _supabase
                .From<PlayerStat>()
                .Where(p => p.Id != null)
                .Delete();

            return Ok(new { message = "All player stats deleted successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting player stats: {ex.Message}");
        }
    }

    // POST route for adding all player stats (ASSUMES players already in the database)
    // meant to be called ONE TIME before the update cycle begins
    [HttpPost("all")]
    public async Task<IActionResult> PostAll()
    {
        try
        {
            // fetch offensive players using service
            var seasonStats = await _nflVerseService.GetAllOffensivePlayerSeasonStatsAsync();

            if (seasonStats == null || !seasonStats.Any())
            {
                return BadRequest("No season stats found to insert.");
            }

            // dictionary to store the new player stat ids
            var playerStatIds = new Dictionary<string, Guid>();

            // convert the PlayerStatCsv to PlayerStat Supabase model for insertion
            var stats = seasonStats
                .Select(p => {
                    var id = Guid.NewGuid();
                    playerStatIds[p.PlayerId] = id;

                    return new PlayerStat
                    {
                        Id = id,
                        Completions = ControllerHelpers.NullIfZero(p.Completions),
                        PassingAttempts = ControllerHelpers.NullIfZero(p.PassingAttempts),
                        PassingYards = ControllerHelpers.NullIfZero(p.PassingYards),
                        PassingTds = ControllerHelpers.NullIfZero(p.PassingTds),
                        InterceptionsAgainst = ControllerHelpers.NullIfZero(p.InterceptionsAgainst),
                        SacksAgainst = ControllerHelpers.NullIfZero(p.SacksAgainst),
                        FumblesAgainst = ControllerHelpers.NullIfZero(p.FumblesAgainst),
                        PassingFirstDowns = ControllerHelpers.NullIfZero(p.PassingFirstDowns),
                        PassingEpa = ControllerHelpers.NullIfZero(p.PassingEpa),
                        Carries = ControllerHelpers.NullIfZero(p.Carries),
                        RushingYards = ControllerHelpers.NullIfZero(p.RushingYards),
                        RushingTds = ControllerHelpers.NullIfZero(p.RushingTds),
                        RushingFirstDowns = ControllerHelpers.NullIfZero(p.RushingFirstDowns),
                        RushingEpa = ControllerHelpers.NullIfZero(p.RushingEpa),
                        Receptions = ControllerHelpers.NullIfZero(p.Receptions),
                        Targets = ControllerHelpers.NullIfZero(p.Targets),
                        ReceivingYards = ControllerHelpers.NullIfZero(p.ReceivingYards),
                        ReceivingTds = ControllerHelpers.NullIfZero(p.ReceivingTds),
                        ReceivingFirstDowns = ControllerHelpers.NullIfZero(p.ReceivingFirstDowns),
                        ReceivingEpa = ControllerHelpers.NullIfZero(p.ReceivingEpa),
                        FgMadeList = p.FgMadeList,
                        FgMissedList = p.FgMissedList,
                        FgBlockedList = p.FgBlockedList,
                        PadAttempts = ControllerHelpers.NullIfZero(p.PadAttempts),
                        PatPercent = ControllerHelpers.NullIfZero(p.PatPercent),
                        FantasyPoints = p.FantasyPoints,
                        FantasyPointsPpr = p.FantasyPointsPpr
                    };
                })
                .ToList();

            var response = await _supabase
                .From<PlayerStat>()
                .Insert(stats);

            // update every player's season_stats_id to point to the season stats
            // serialize the player id and stat id pairs
            var pairs = playerStatIds
                .Select(kvp => new { player_id = kvp.Key, stat_id = kvp.Value })
                .ToList();

            // call predefined Postgres function to batch update the user ids
            await _supabase.Rpc("batch_update_season_stats_for_players", new { pairs });

            return Ok(new { message = "Season stats inserted successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error populating season stats: {ex.Message}");
        }
    }

    // PUT route for updating all player stats (MAIN route to be called from scheduled Azure Functions App)
    [HttpPut("all")]
    public async Task<IActionResult> PutAll()
    {
        try
        {
            // fetch offensive players using service
            var seasonStats = await _nflVerseService.GetAllOffensivePlayerSeasonStatsAsync();

            if (seasonStats == null || !seasonStats.Any())
            {
                return BadRequest("No season stats found to insert.");
            }

            // get pairs of player_id and their season_stats_id
            var playerIds = seasonStats.Select(s => s.PlayerId).ToList();

            var playersResponse = await _supabase
                .From<Player>()
                .Select(p => new object[] { p.Id, p.SeasonStatsId })
                .Filter("id", Supabase.Postgrest.Constants.Operator.In, playerIds)
                .Get();

            var playerStatMap = playersResponse.Models
                .Where(p => p.SeasonStatsId != null)
                .ToDictionary(p => p.Id, p => p.SeasonStatsId.Value);

            // Build list of updates
            var updates = seasonStats.Select(s => new
            {
                season_stats_id = playerStatMap[s.PlayerId],
                completions = ControllerHelpers.NullIfZero(s.Completions),
                passing_attempts = ControllerHelpers.NullIfZero(s.PassingAttempts),
                passing_yards = ControllerHelpers.NullIfZero(s.PassingYards),
                passing_tds = ControllerHelpers.NullIfZero(s.PassingTds),
                interceptions_against = ControllerHelpers.NullIfZero(s.InterceptionsAgainst),
                sacks_against = ControllerHelpers.NullIfZero(s.SacksAgainst),
                fumbles_against = ControllerHelpers.NullIfZero(s.FumblesAgainst),
                passing_first_downs = ControllerHelpers.NullIfZero(s.PassingFirstDowns),
                passing_epa = ControllerHelpers.NullIfZero(s.PassingEpa),
                carries = ControllerHelpers.NullIfZero(s.Carries),
                rushing_yards = ControllerHelpers.NullIfZero(s.RushingYards),
                rushing_tds = ControllerHelpers.NullIfZero(s.RushingTds),
                rushing_first_downs = ControllerHelpers.NullIfZero(s.RushingFirstDowns),
                rushing_epa = ControllerHelpers.NullIfZero(s.RushingEpa),
                receptions = ControllerHelpers.NullIfZero(s.Receptions),
                targets = ControllerHelpers.NullIfZero(s.Targets),
                receiving_yards = ControllerHelpers.NullIfZero(s.ReceivingYards),
                receiving_tds = ControllerHelpers.NullIfZero(s.ReceivingTds),
                receiving_first_downs = ControllerHelpers.NullIfZero(s.ReceivingFirstDowns),
                receiving_epa = ControllerHelpers.NullIfZero(s.ReceivingEpa),
                fg_made_list = s.FgMadeList,
                fg_missed_list = s.FgMissedList,
                fg_blocked_list = s.FgBlockedList,
                pat_attempts = ControllerHelpers.NullIfZero(s.PadAttempts),
                pat_percent = ControllerHelpers.NullIfZero(s.PatPercent),
                fantasy_points = s.FantasyPoints,
                fantasy_points_ppr = s.FantasyPointsPpr
            }).ToList();

            // Call RPC to update all at once
            await _supabase.Rpc("batch_update_player_season_stats", new { updates });

            return Ok(new { message = "Season stats updated successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating season stats: {ex.Message}");
        }
    }
}
