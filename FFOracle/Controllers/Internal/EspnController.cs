using FFOracle.Models.Supabase;
using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using FFOracle.DTOs.Responses;

namespace FFOracle.Controllers.Internal;

[ApiController]
[Route("api/[controller]")]
public class EspnController: ControllerBase
{
    private readonly EspnService _espnService;
    private readonly SupabaseAuthService _authService;
    private readonly Client _supabase;

    public EspnController(EspnService espnService, SupabaseAuthService authService, Client supabase)
    {
        _espnService = espnService;
        _authService = authService;
        _supabase = supabase;
    }

    // POST route to update the player_espn_id table
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

    // GET route to fetch a user's list of articles for their league
    [HttpGet("articles/{leagueId}")]
    public async Task<IActionResult> GetArticlesForLeague(Guid leagueId, [FromQuery] int days = 30)
    {
        try
        {
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == null)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            // 2. ENSURE LEAGUE BELONGS TO USER
            var leagueRes = await _supabase
                .From<UserLeague>()
                .Where(l => l.Id == leagueId && l.UserID == userId)
                .Single();

            if (leagueRes == null)
            {
                return NotFound(new { message = "League not found for user." });
            }

            // 3. GET ALL OFFENSIVE PLAYERS IN THIS LEAGUE
            var memberRes = await _supabase
                .From<LeagueOffensiveMember>()
                .Where(m => m.LeagueId == leagueId)
                .Get();

            var members = memberRes.Models;

            if (members.Count == 0)
            {
                return Ok(new List<EspnNewsItem>()); // no players = no articles
            }

            // used to aggregate all articles
            var articles = new List<EspnNewsItem>();

            // aggregate all articles for the league
            foreach (var player in members)
            {
                // fetch ESPN articles using PlayerId (uuid)
                var playerArticles = await _espnService.GetArticlesForPlayerAsync(player.PlayerId);
                articles.AddRange(playerArticles);
            }

            var cutoff = DateTime.UtcNow.AddDays(-days);

            // 5. SORT BY PUBLISHED DATE DESC
            articles = articles
                .Where(a => a.Published != null && a.Published >= cutoff)
                .OrderByDescending(a => a.Published)
                .ToList();

            // return all articles
            return Ok(articles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error fetching articles for league {leagueId}: {ex.Message}" });
        }
    }
}
