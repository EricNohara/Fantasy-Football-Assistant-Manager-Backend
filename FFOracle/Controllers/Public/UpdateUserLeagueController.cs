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

    //DELETE route
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

    //UPDATE route
    [HttpPut]
    public async Task<IActionResult> UpdateLeague([FromBody] DTOs.LeagueUpdate info)
    {

        //Turn the update info into a json string, then convert to a json element to be passed into the rpc
        var json = JsonSerializer.Serialize(info);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        await _supabase.Rpc("update_league", new { updateinfo = element });

        ////Pass an abbreviated set of league info for Supabase to act on via rpc
        //await _supabase.Rpc(
        //    "update_league",
        //    new { updateinfo = info }
        //    );
        return Ok();
    }
}

