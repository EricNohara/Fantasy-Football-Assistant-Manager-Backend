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

    // injects NflVerseService via dependency injection
    public PlayerWeeklyStatController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    // POST route for adding all weekly player stats up to the most recent week
    // to be called from Azure Functions App on a schedule for weekly updates
    [HttpPost("all")]
    public async Task<IActionResult> PostAll()
    {
        try
        {
            var weeklyStats = await _nflVerseService.GetAllOffensivePlayerWeeklyStatsAsync();

            if (weeklyStats == null || !weeklyStats.Any())
            {
                return BadRequest("No season stats found to insert.");
            }

            // Get the most latest week's data stored in the db
            var latestWeekResponse = await _supabase
                .From<WeeklyPlayerStat>()
                .Select("week")
                .Order("week", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            // set the week to 0 if there are none in the db currently
            var latestWeek = latestWeekResponse.Models.FirstOrDefault()?.Week ?? 0;

            // find the latest week in new data
            int newLatestWeek = weeklyStats.Max(s => s.Week);

            // only insert the weeks AFTER the last recorded one
            var newStats = weeklyStats
                .Where(s => s.Week > latestWeek)
                .ToList();

            if (!newStats.Any())
            {
                return Ok(new { message = $"No new weekly stats to insert (latest week: {latestWeek})" });
            }

            // convert the newStats to PlayerStats model while generating list of WeeklyPlayerStat models
            List<WeeklyPlayerStat> weeklyPlayerStats = new List<WeeklyPlayerStat>();

            var playerStats = newStats
                .Select(p =>
                {
                    var id = Guid.NewGuid();

                    // create and insert new WeeklyPlayerStat model
                    var weeklyStat = new WeeklyPlayerStat
                    {
                        Id = Guid.NewGuid(),
                        PlayerStatsId = id,
                        PlayerId = p.PlayerId,
                        Week = p.Week,
                        SeasonStartYear = p.SeasonStartYear,
                    };

                    weeklyPlayerStats.Add(weeklyStat);

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

            // insert PlayerStat models into player_stats table
            var insertedStatsResponse = await _supabase
                .From<PlayerStat>()
                .Insert(playerStats);

            // insert new relational record into weekly_player_stats table
            var insertedWeeklyResponse = await _supabase
                .From<WeeklyPlayerStat>()
                .Insert(weeklyPlayerStats);

            return Ok(new { message = $"Inserted weekly stats successfully for weeks {latestWeek + 1}–{newStats.Max(s => s.Week)}" });
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
