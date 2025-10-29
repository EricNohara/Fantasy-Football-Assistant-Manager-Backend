using Fantasy_Football_Assistant_Manager.Models;
using Fantasy_Football_Assistant_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeasonStatController: ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;

    // injects NflVerseService via dependency injection
    public SeasonStatController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    // helper function for data cleaning
    private static int? NullIfZero(int? value) => value == 0 ? null : value;
    private static float? NullIfZero(float? value) => value == 0f ? null : value;

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
            var seasonStats = await _nflVerseService.GetAllOffensiveSeasonStatsAsync();

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
                        Completions = NullIfZero(p.Completions),
                        PassingAttempts = NullIfZero(p.PassingAttempts),
                        PassingYards = NullIfZero(p.PassingYards),
                        PassingTds = NullIfZero(p.PassingTds),
                        InterceptionsAgainst = NullIfZero(p.InterceptionsAgainst),
                        SacksAgainst = NullIfZero(p.SacksAgainst),
                        FumblesAgainst = NullIfZero(p.FumblesAgainst),
                        PassingFirstDowns = NullIfZero(p.PassingFirstDowns),
                        PassingEpa = NullIfZero(p.PassingEpa),
                        Carries = NullIfZero(p.Carries),
                        RushingYards = NullIfZero(p.RushingYards),
                        RushingTds = NullIfZero(p.RushingTds),
                        RushingFirstDowns = NullIfZero(p.RushingFirstDowns),
                        RushingEpa = NullIfZero(p.RushingEpa),
                        Receptions = NullIfZero(p.Receptions),
                        Targets = NullIfZero(p.Targets),
                        ReceivingYards = NullIfZero(p.ReceivingYards),
                        ReceivingTds = NullIfZero(p.ReceivingTds),
                        ReceivingFirstDowns = NullIfZero(p.ReceivingFirstDowns),
                        ReceivingEpa = NullIfZero(p.ReceivingEpa),
                        FgMadeList = p.FgMadeList,
                        FgMissedList = p.FgMissedList,
                        FgBlockedList = p.FgBlockedList,
                        PadAttempts = NullIfZero(p.PadAttempts),
                        PatPercent = NullIfZero(p.PatPercent),
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
            var seasonStats = await _nflVerseService.GetAllOffensiveSeasonStatsAsync();

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
                completions = NullIfZero(s.Completions),
                passing_attempts = NullIfZero(s.PassingAttempts),
                passing_yards = NullIfZero(s.PassingYards),
                passing_tds = NullIfZero(s.PassingTds),
                interceptions_against = NullIfZero(s.InterceptionsAgainst),
                sacks_against = NullIfZero(s.SacksAgainst),
                fumbles_against = NullIfZero(s.FumblesAgainst),
                passing_first_downs = NullIfZero(s.PassingFirstDowns),
                passing_epa = NullIfZero(s.PassingEpa),
                carries = NullIfZero(s.Carries),
                rushing_yards = NullIfZero(s.RushingYards),
                rushing_tds = NullIfZero(s.RushingTds),
                rushing_first_downs = NullIfZero(s.RushingFirstDowns),
                rushing_epa = NullIfZero(s.RushingEpa),
                receptions = NullIfZero(s.Receptions),
                targets = NullIfZero(s.Targets),
                receiving_yards = NullIfZero(s.ReceivingYards),
                receiving_tds = NullIfZero(s.ReceivingTds),
                receiving_first_downs = NullIfZero(s.ReceivingFirstDowns),
                receiving_epa = NullIfZero(s.ReceivingEpa),
                fg_made_list = s.FgMadeList,
                fg_missed_list = s.FgMissedList,
                fg_blocked_list = s.FgBlockedList,
                pat_attempts = NullIfZero(s.PadAttempts),
                pat_percent = NullIfZero(s.PatPercent),
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
