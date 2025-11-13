using FFOracle.Models.Supabase;
using Microsoft.AspNetCore.Mvc;

namespace FFOracle.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppStateController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    public AppStateController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    [HttpPut("increment")]
    public async Task<IActionResult> UpdateAppState()
    {
        try
        {
            // Get the existing app state row
            var res = await _supabase
                .From<AppState>()
                .Get();

            var appState = res.Models.FirstOrDefault();

            if (appState == null)
            {
                return NotFound("App state not found.");
            }

            if (appState.CurrentWeek < 17)
            {
                appState.CurrentWeek += 1;
            }

            // Persist the changes to Supabase
            var updateRes = await _supabase
                .From<AppState>()
                .Where(a => a.Id == appState.Id)
                .Update(appState);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating app state: {ex.Message}");
        }
    }
}
