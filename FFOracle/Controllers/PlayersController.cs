using FFOracle.Models.Supabase;
using FFOracle.Services;
using FFOracle.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace FFOracle.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController: ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;
    private readonly UpdateSupabaseService _updateSupabaseService;

    // injects NflVerseService via dependency injection
    public PlayersController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
        _updateSupabaseService = new UpdateSupabaseService(_nflVerseService, _supabase);
    }

    // POST route for adding players
    [HttpPost("all")]
    public async Task<IActionResult> PostAll ()
    {
        try
        {
            // fetch offensive players using service
            var players = await _nflVerseService.GetAllOffensivePlayersAsync();
            if (players == null || !players.Any())
            {
                return BadRequest("No players found to insert.");
            }

            // get the current season and week from supabase
            var (currentSeason, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);

            // fetch player information using service
            var playerInformation = await _nflVerseService.GetAllPlayerInformationAsync(currentSeason);
            if (playerInformation == null || !playerInformation.Any())
            {
                return BadRequest("No player information found.");
            }

            // filter out player information for players who are not in the fetched players
            var playerLookup = players.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

            // keep only information for players that exist in the players list and generate Player models from them
            var filteredPlayerWithInfo = playerInformation
                .Where(info => !string.IsNullOrWhiteSpace(info.Id) && playerLookup.ContainsKey(info.Id))
                .Select(info =>
                {
                    var player = playerLookup[info.Id]; // get the matching player
                    return new Player
                    {
                        Id = info.Id,
                        Name = player.Name,
                        HeadshotUrl = player.HeadshotUrl,
                        Position = player.Position,
                        Status = info.Status,
                        StatusDescription = info.ShortDescription,
                        TeamId = info.LatestTeam,
                        SeasonStatsId = player.SeasonStatsId,
                    };
                })
                .ToList();

            // insert into Supabase players table
            var response = await _supabase
                .From<Player>()
                .Insert(filteredPlayerWithInfo);

            return Ok(new { message = "Players inserted successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error populating players: {ex.Message}");
        }
    }

    // DELETE route for deleting all players
    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll ()
    {
        try
        {
            // Delete all records from the players table
            await _supabase
                .From<Player>()
                .Where(p => p.Id != null)
                .Delete();

            return Ok(new { message = "All players deleted successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting players: {ex.Message}");
        }
    }

    // PUT route to update all player information (not their stats)
    // used to update injury status/current team to be called from Functions App
    [HttpPut("all")]
    public async Task<IActionResult> PutAll()
    {
        try
        {
            await _updateSupabaseService.UpdateAllPlayerNonStatDataAsync();
            return Ok(new { message = "Player information updated successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating all player information: {ex.Message}");
        }
    }
}
