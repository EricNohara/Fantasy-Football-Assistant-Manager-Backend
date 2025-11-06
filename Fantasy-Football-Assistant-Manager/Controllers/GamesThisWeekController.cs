using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Services;
using Fantasy_Football_Assistant_Manager.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesThisWeekController : ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;

    public GamesThisWeekController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    [HttpPost("all")]
    public async Task<IActionResult> PostAll()
    {
        try
        {
            // get current season and week from supabase
            var (currentSeason, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);

            // get game data from nfl verse service
            var games = await _nflVerseService.GetAllGamesThisWeekAsync(currentSeason, currentWeek);

            if (games == null || !games.Any())
            {
                return BadRequest(new { message = "No games found for current week." });
            }

            // map GameThisWeekCsv to GameThisWeek supabase model
            var gamesToInsert = games.Select(g => new GameThisWeek
            {
                Id = Guid.NewGuid(),
                HomeTeam = g.HomeTeam,
                AwayTeam = g.AwayTeam,
                Weekday = g.Weekday,
                GameDateTime = g.GameDateTime,
                StadiumName = g.StadiumName,
                StadiumStyle = g.StadiumStyle,
                IsDivisionalGame = g.IsDivisionalGame,
                HomeRestDays = g.HomeRestDays,
                AwayRestDays = g.AwayRestDays,
                HomeMoneyline = g.HomeMoneyline,
                AwayMoneyline = g.AwayMoneyline,
                HomeSpreadOdds = g.HomeSpreadOdds,
                AwaySpreadOdds = g.AwaySpreadOdds,
                SpreadLine = g.SpreadLine,
                TotalLine = g.TotalLine,
                UnderOdds = g.UnderOdds,
                OverOdds = g.OverOdds
            }).ToList();

            // insert models into supabase
            await _supabase
                .From<GameThisWeek>()
                .Insert(gamesToInsert);

            return Ok(new { message = "Successfully inserted all games this week" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error inserting all games this week: {ex.Message}");
        }
    }

    [HttpPut("all")]
    public async Task<IActionResult> PutAll()
    {
        try
        {
            // get current season and week from supabase
            var (currentSeason, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);

            // get game data from nfl verse service
            var games = await _nflVerseService.GetAllGamesThisWeekAsync(currentSeason, currentWeek);

            if (games == null || !games.Any())
            {
                return BadRequest(new { message = "No games found for current week." });
            }

            // map GameThisWeekCsv to GameThisWeek supabase model
            var gamesToInsert = games.Select(g => new GameThisWeek
            {
                Id = Guid.NewGuid(),
                HomeTeam = g.HomeTeam,
                AwayTeam = g.AwayTeam,
                Weekday = g.Weekday,
                GameDateTime = g.GameDateTime,
                StadiumName = g.StadiumName,
                StadiumStyle = g.StadiumStyle,
                IsDivisionalGame = g.IsDivisionalGame,
                HomeRestDays = g.HomeRestDays,
                AwayRestDays = g.AwayRestDays,
                HomeMoneyline = g.HomeMoneyline,
                AwayMoneyline = g.AwayMoneyline,
                HomeSpreadOdds = g.HomeSpreadOdds,
                AwaySpreadOdds = g.AwaySpreadOdds,
                SpreadLine = g.SpreadLine,
                TotalLine = g.TotalLine,
                UnderOdds = g.UnderOdds,
                OverOdds = g.OverOdds
            }).ToList();

            // clear the games this week table and repopulate it
            await _supabase
                .From<GameThisWeek>()
                .Where(g => g.Id != null)
                .Delete();

            await _supabase
                .From<GameThisWeek>()
                .Insert(gamesToInsert);

            return Ok(new { message = "Successfully updated all games this week" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updated all games this week: {ex.Message}");
        }
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            await _supabase
                .From<GameThisWeek>()
                .Where(g => g.Id != null)
                .Delete();

            return Ok(new { message = "Successfully deleted all games this week" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting all games this week: {ex.Message}");
        }
    }
}
