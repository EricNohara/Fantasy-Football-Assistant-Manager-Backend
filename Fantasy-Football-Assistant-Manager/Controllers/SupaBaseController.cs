using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupaBaseController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    // Allowed tables that this controller may expose. Update as needed.
    private static readonly HashSet<string> AllowedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "users",
        "players",
        "player_stats",
        "teams",
        "team_members",
        "weekly_player_stats",
        "scoring_settings",
        "roster_settings",
        "team_offensive_stats",
        "team_defensive_stats"
    };

    public SupaBaseController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    /// <summary>
    /// Generic read-only endpoint to fetch all rows from a small, allow-listed set of Supabase tables.
    /// This maps table names to typed model queries to preserve schema and serialization behavior.
    /// </summary>
    [HttpGet("{table}")]
    public async Task<IActionResult> GetTableData(string table)
    {
        if (string.IsNullOrWhiteSpace(table) || !AllowedTables.Contains(table))
            return BadRequest(new { error = "Invalid or disallowed table name" });

        try
        {
            // Map table name to typed Supabase model queries so results are strongly typed.
            switch (table.ToLowerInvariant())
            {
                case "users":
                {
                    var res = await _supabase.From<Models.User>().Get();
                    return Ok(res);
                }
                case "players":
                {
                    var res = await _supabase.From<Models.Player>().Get();
                    return Ok(res);
                }
                case "player_stats":
                {
                    var res = await _supabase.From<Models.PlayerStat>().Get();
                    return Ok(res);
                }
                case "teams":
                {
                    var res = await _supabase.From<Models.Team>().Get();
                    return Ok(res);
                }
                case "team_members":
                {
                    var res = await _supabase.From<Models.TeamMember>().Get();
                    return Ok(res);
                }
                case "weekly_player_stats":
                {
                    var res = await _supabase.From<Models.WeeklyPlayerStat>().Get();
                    return Ok(res);
                }
                case "scoring_settings":
                {
                    var res = await _supabase.From<Models.ScoringSetting>().Get();
                    return Ok(res);
                }
                case "roster_settings":
                {
                    var res = await _supabase.From<Models.RosterSetting>().Get();
                    return Ok(res);
                }
                case "team_offensive_stats":
                {
                    var res = await _supabase.From<Models.TeamOffensiveStat>().Get();
                    return Ok(res);
                }
                case "team_defensive_stats":
                {
                    var res = await _supabase.From<Models.TeamDefensiveStat>().Get();
                    return Ok(res);
                }
                default:
                    return BadRequest(new { error = "Unhandled table" });
            }
        }
        catch (Exception ex)
        {
            // Keep the error message concise while still exposing helpful info for server-side debugging.
            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}