using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using System.Text.Json;


//A controller to retrieve all data specific to a certain user
//Based on the SupeBaseController code used for our test app

namespace FFOracle.Controllers.SupabaseControllers;

[ApiController]
[Route("api/[controller]")]
public class GetPlayersByPositionController : ControllerBase
{
    private readonly Client _supabase;
    private readonly SupabaseAuthService _authService;

    public GetPlayersByPositionController(Client supabase, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _authService = authService;
    }

    // Read-only endpoint to fetch all players of a certain position.
    // If the position is defense, it instead returns all teams.
    [HttpGet("{position}")]
    public async Task<IActionResult> GetPlayersByPosition(String position)
    {
        try
        {
            // authorize the user
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == Guid.Empty)
            {
                return Unauthorized("Invalid token");
            }

            //if the defense has been requested, return a list of teams since defense
            // is per-team
            if (position.Equals("DEF"))
            {
                var result = await _supabase.Rpc("get_defense_teams", new { });
                //parse the content field to JSON
                using var doc = JsonDocument.Parse(result.Content.ToString());
                var root = doc.RootElement;
                //pretty-print the result
                var prettyDoc = JsonSerializer.Serialize(root, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return Ok(prettyDoc);
            }
            //else, return a list of players of that particular position
            else
            {
                var result = await _supabase.Rpc("get_players_by_position", new { _position = position });
                //parse the content field to JSON
                using var doc = JsonDocument.Parse(result.Content.ToString());
                var root = doc.RootElement;
                //pretty-print the result
                var prettyDoc = JsonSerializer.Serialize(root, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return Ok(prettyDoc);
            }

        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}