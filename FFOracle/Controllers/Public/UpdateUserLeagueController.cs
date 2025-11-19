using FFOracle.Models.Supabase;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using System.Text.Json;
using FFOracle.DTOs.Requests;
using FFOracle.Services;

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]

public class UpdateUserLeagueController : Controller
{
    private readonly Client _supabase;
    private readonly SupabaseAuthService _authService;

    public UpdateUserLeagueController(Client supabase, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _authService = authService;
    }

    //delete league route
    [HttpDelete]
    public async Task<IActionResult> DeleteLeague(Guid leagueId)
    {
        try
        {
            // check user is authenticated
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            // only league owner can delete league
            var leagueOwnerRes = await _supabase
                .From<UserLeague>()
                .Where(ul => ul.Id == leagueId && ul.UserID == userId)
                .Get();

            if (leagueOwnerRes.Models.Count == 0)
            {
                return Forbid();
            }

            // Delete the league 
            await _supabase.Rpc("delete_league", new { _league_id = leagueId });

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error authenticating user", details = ex.Message });
        }
    }

    //update scoring settings route
    [HttpPut("updateScoringSettings")]
    public async Task<IActionResult> UpdateScoringSettings([FromBody] UpdateScoringSettingsRequest req)
    {
        try
        {
            // check user is authenticated
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            // check if user is league owner
            var leagueOwnerRes = await _supabase
                .From<UserLeague>()
                .Where(ul => ul.Id == req.LeagueId && ul.UserID == userId)
                .Get();

            if (leagueOwnerRes.Models.Count == 0)
            {
                return NotFound(new { error = "League not found or you are not the owner" });
            }

            // Convert request to json
            var newSettings = new
            {
                points_per_td = req.PointsPerTd,
                points_per_reception = req.PointsPerReception,
                points_per_passing_yard = req.PointsPerPassingYard,
                points_per_rushing_yard = req.PointsPerRushingYard,
                points_per_reception_yard = req.PointsPerReceptionYard
            };

            // Call RPC
            await _supabase.Rpc("update_league_scoring", new
            {
                league_id = req.LeagueId,
                new_settings = newSettings
            });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error updating scoring settings", details = ex.Message });
        }
    }

    //update roster settings route
    [HttpPut("updateRosterSettings")]
    public async Task<IActionResult> UpdateRosterSettings(
        [FromBody] DTOs.RosterSetting new_settings
        )
    {
        //Turn the update info into a json string, then convert to a
        // json element to be passed into the rpc
        var json = JsonSerializer.Serialize(new_settings);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        await _supabase.Rpc(
            "update_roster_settings",
            new
            {
                new_settings
            }
            );

        return Ok();
    }

    //update member picked status route
    [HttpPut("updatePickedMemberStatus")]
    public async Task<IActionResult> UpdatePickedLeagueMembers(
        [FromBody] DTOs.LeagueMemberLists member_lists,
        Guid league_id
    ){
        //Turn the update info into a json string, then convert to a
        // json element to be passed into the rpc
        var json = JsonSerializer.Serialize(member_lists);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        await _supabase.Rpc(
            "update_picked_league_members", 
            new { 
                member_lists,
                league_id
            }
            );

        return Ok();
    }

    //add member route
    [HttpPut("addMembers")]
    public async Task<IActionResult> AddLeagueMembers(
        [FromBody] DTOs.LeagueMemberLists member_lists,
        Guid league_id
    )
    {
        //Turn the update info into a json string, then convert to a
        // json element to be passed into the rpc
        var json = JsonSerializer.Serialize(member_lists);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        await _supabase.Rpc(
            "add_league_members",
            new
            {
                member_lists,
                league_id
            }
            );

        return Ok();
    }

    //delete member route
    [HttpPut("deleteMembers")]
    public async Task<IActionResult> DeleteLeagueMembers(
        [FromBody] DTOs.LeagueMemberLists member_lists,
        Guid league_id
    )
    {
        //Turn the update info into a json string, then convert to a
        // json element to be passed into the rpc
        var json = JsonSerializer.Serialize(member_lists);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        await _supabase.Rpc(
            "delete_league_members",
            new
            {
                member_lists,
                league_id
            }
            );

        return Ok();
    }
}

