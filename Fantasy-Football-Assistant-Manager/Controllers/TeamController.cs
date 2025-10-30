using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;


namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;

    // injects NflVerseService via dependency injection
    public TeamController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    // POST route for posting all teams to the teams table
    // meant to be called once on startup
    [HttpPost("all")]
    public async Task<IActionResult> PostAll()
    {
        try
        {
            var teams = await _nflVerseService.GetAllTeamDataAsync();

            if (teams == null || !teams.Any())
            {
                return BadRequest("No teams found to insert.");
            }

            // map TeamDataCsv to Team model
            var teamsToInsert = teams
                .Select(t => new Team
                {
                    Id = t.Id,
                    Name = t.Name,
                    Conference = t.Conference,
                    Division = t.Division,
                    LogoUrl = t.LogoUrl,
                    DefensiveStatsId = null,
                    OffensiveStatsId = null
                })
                .ToList();

            var response = await _supabase
                .From<Team>()
                .Insert(teamsToInsert);

            return Ok(new { message = "All teams inserted successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error inserting all teams: {ex.Message}");
        }
    }

}