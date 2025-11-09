using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Responses;
using static Supabase.Postgrest.Constants;

//A controller to retrieve all data specific to a certain user
//Based on the SupeBaseController code used for our test app

namespace Fantasy_Football_Assistant_Manager.Controllers.SupaBaseControllers;

[ApiController]
[Route("api/[controller]")]
public class GetPlayersByPositionController : ControllerBase
{
    private readonly Client _supabase;

    public GetPlayersByPositionController(Client supabase)
    {
        _supabase = supabase;
    }

    // Read-only endpoint to fetch all players of a certain position.
    // If the position is defense, it instead returns all teams.
    [HttpGet("{position}")]
    public async Task<IActionResult> GetPlayersByPosition(String position)
    {
        try
        {
            //if the defense has been requested, return a list of teams since defense
            // is per-team
            if (position.Equals("DEF"))
            {
                var teams = await _supabase.From<Team>().Get();
                var teamDTOs = teams.Models.Select(m => new DTOs.Team
                {
                    Id = m.Id,
                    Name = m.Name,
                    OffensiveStatsId = m.OffensiveStatsId,
                    DefensiveStatsId = m.DefensiveStatsId
                }).ToList();

                return Ok(teamDTOs);
            } 
            //else, return a list of players of that particular position
            else
            {
                //the * indicates all fields.
                //since there is a foreign key for stats in players, I can join the two tables
                // implicitly like this.
                var players = await _supabase
                    .From<Player>()
                    //.Select("*, season_stats(*)")   
                    .Where(p => p.Position == position)
                    .Get();

                var playerDTOs = players.Models.Select(m => new DTOs.Player
                {
                    Id = m.Id,
                    Name = m.Name,
                    HeadshotUrl = m.HeadshotUrl,
                    Position = m.Position,
                    Status = m.Status,
                    StatusDescription = m.StatusDescription,
                    TeamId = m.TeamId,
                    SeasonStatsId = m.SeasonStatsId
                }).ToList();

                return Ok(playerDTOs);
            }
        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}
