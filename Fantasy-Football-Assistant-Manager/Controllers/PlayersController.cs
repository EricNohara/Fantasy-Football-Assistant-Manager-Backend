using Fantasy_Football_Assistant_Manager.Models;
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

            // initialize Supabase client connection
            await _supabase.InitializeAsync();

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
            // Initialize Supabase client
            await _supabase.InitializeAsync();

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
}
