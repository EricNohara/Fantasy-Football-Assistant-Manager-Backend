using FFOracle.Models.Supabase;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using System.Text.Json;

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]

public class UpdateUserLeagueController : Controller
{
    private readonly Client _supabase;
    public UpdateUserLeagueController(Client supabase)
    {
        _supabase = supabase;
    }

    //delete league route
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLeague(Guid id)
    {
        //Delete the league with the id given. Any cascading is done by Supabase. 
        await _supabase
            .From<UserLeague>()
            .Where(t => t.Id == id)
            .Delete();

        return Ok();
    }

    //update scoring settings route
    [HttpPut("updateScoring")]
    public async Task<IActionResult> UpdateScoringSettings(
        [FromBody] DTOs.ScoringSetting new_settings
        )
    {
        //Turn the update info into a json string, then convert to a
        // json element to be passed into the rpc
        var json = JsonSerializer.Serialize(new_settings);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        await _supabase.Rpc(
            "update_scoring_settings",
            new
            {
                new_settings
            }
            );

        return Ok();
    }

    //update roster settings route
    [HttpPut("updateRoster")]
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
    [HttpPut("updatePickedMembers")]
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

