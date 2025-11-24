using FFOracle.Models.Supabase;
using FFOracle.Services;
using FFOracle.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace FFOracle.Controllers.Internal;

[ApiController]
[Route("api/[controller]")]
public class EspnController: ControllerBase
{
    private readonly EspnService _espnService;
    public EspnController(EspnService espnService)
    {
        _espnService = espnService;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncPlayers()
    {
        try
        {
            await _espnService.SyncAllPlayersAsync();
            return Ok(new { message = "ESPN players synced successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error syncing ESPN players: {ex.Message}" });
        }
    }
}
