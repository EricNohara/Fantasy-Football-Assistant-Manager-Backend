using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController: ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;

    // injects NflVerseService via dependency injection
    public PlayersController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    // ALL PLAYERS ROUTES
    // GET route for getting all players
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var response = await _supabase
                .From<Player>()
                .Get();

            var players = response.Models
            .Select(p => new DTOs.Player
            {
                Id = p.Id,
                Name = p.Name,
                HeadshotUrl = p.HeadshotUrl,
                Position = p.Position,
                InjuryStatus = p.InjuryStatus,
                TeamId = p.TeamId,
                SeasonStatsId = p.SeasonStatsId
            })
            .ToList();

            return Ok(players);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting all players: {ex.Message}");
        }
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
                return BadRequest("No players found to insert.");

            // insert into Supabase players table
            var response = await _supabase
                .From<Player>()
                .Insert(players);

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

    // SINGLE PLAYER ROUTE
    [HttpGet("{playerId}")]
    public async Task<IActionResult> GetSingle(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return BadRequest(new { error = "Invalid or player id" });
        }

        try
        {
            await _supabase.InitializeAsync();

            var response = await _supabase
                .From<Player>()
                .Where(p => p.Id == playerId)
                .Get();

            var supabasePlayer = response.Models.FirstOrDefault();

            if (supabasePlayer == null)
            {
                return NotFound(new { error = $"Player with id {playerId} not found" });
            }

            var player = new DTOs.Player
            {
                Id = supabasePlayer.Id,
                Name = supabasePlayer.Name,
                HeadshotUrl = supabasePlayer.HeadshotUrl,
                Position = supabasePlayer.Position,
                InjuryStatus = supabasePlayer.InjuryStatus,
                TeamId = supabasePlayer.TeamId,
                SeasonStatsId = supabasePlayer.SeasonStatsId
            };

            return Ok(player);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting player with Id {playerId}: {ex.Message}");
        }
    }
}
