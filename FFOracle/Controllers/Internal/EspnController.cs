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
    private readonly EmailService _emailService;

    public EspnController(EspnService espnService, SupabaseAuthService authService, Client supabase, EmailService emailService)
    {
        _espnService = espnService;
        _authService = authService;
        _supabase = supabase;
        _emailService = emailService;
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

            // filter out duplicate articles
            var uniqueArticles = articles
                .GroupBy(a => a.Headline)
                .Select(g => g.First())
                .ToList();

            // return all articles
            return Ok(uniqueArticles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error fetching articles for league {leagueId}: {ex.Message}" });
        }
    }

    [HttpPost("emailArticles")]
    public async Task<IActionResult> SendAllArticleEmails()
    {
        try
        {
            // get all user ids from supabase who have allow emails enabled
            var userRes = await _supabase
                .From<User>()
                .Where(u => u.AllowEmails == true)
                .Get();

            var users = userRes.Models;
            if (users.Count == 0)
            {
                return Ok("No users with email notifications enabled.");
            }

            int totalEmailsSent = 0;

            // LOOP THROUGH EACH USER
            foreach (var user in users)
            {
                var userId = user.Id;
                var userEmail = user.Email;

                // Get all leagues for this user
                var leaguesRes = await _supabase
                    .From<UserLeague>()
                    .Where(l => l.UserID == userId)
                    .Select("id, name")
                    .Get();

                var leagues = leaguesRes.Models;

                // Process each league
                foreach (var league in leagues)
                {
                    // CALL YOUR EXISTING ARTICLE LOGIC:
                    var articles = await GetArticlesForLeagueInternal(league.Id, 1, userId);

                    if (articles.Count == 0)
                        continue; // no email needed

                    // Build the HTML email
                    var htmlBody = BuildLeagueArticlesHtml(league.Name, articles);

                    // Send the email
                    await _emailService.SendEmailAsync(
                        userEmail,
                        $"Your FFOracle Daily News — {league.Name}",
                        htmlBody
                    );

                    totalEmailsSent++;
                }
            }

            return Ok($"{totalEmailsSent} article emails sent.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error sending ESPN article emails: {ex.Message}" });
        }
    }

    // helpers
    private async Task<List<EspnNewsItem>> GetArticlesForLeagueInternal(Guid leagueId, int days, Guid userId)
    {
        // verify league belongs to the user
        var leagueRes = await _supabase
            .From<UserLeague>()
            .Where(l => l.Id == leagueId && l.UserID == userId)
            .Single();

        if (leagueRes == null)
            return new List<EspnNewsItem>();

        // get league members
        var memberRes = await _supabase
            .From<LeagueOffensiveMember>()
            .Where(m => m.LeagueId == leagueId)
            .Get();

        var members = memberRes.Models;

        var articles = new List<EspnNewsItem>();
        foreach (var m in members)
        {
            var playerArticles = await _espnService.GetArticlesForPlayerAsync(m.PlayerId);
            articles.AddRange(playerArticles);
        }

        var deduped = articles
            .GroupBy(a => a.Headline) 
            .Select(g => g.First())
            .ToList();

        // Filter & sort
        var cutoff = DateTime.UtcNow.AddDays(-days);

        return deduped
            .Where(a => a.Published != null && a.Published >= cutoff)
            .OrderByDescending(a => a.Published)
            .ToList();
    }

    private string BuildLeagueArticlesHtml(string leagueName, List<EspnNewsItem> articles)
    {
        var html = $@"
<div style='font-family:Arial, sans-serif; line-height:1.6; color:#222;'>
    <h2 style='color:#111; margin-bottom:6px;'>FFOracle Daily News Digest</h2>
    <h3 style='color:#444; margin-top:0;'>League: {leagueName}</h3>
    <p style='font-size:14px; color:#555;'>Here are the latest player updates from the last 24 hours:</p>
    <hr style='border:none; border-top:1px solid #ddd; margin:18px 0;'/>
";

        foreach (var a in articles)
        {
            var date = a.Published?.ToString("MMM dd, yyyy") ?? "";
            var img = a.Images?.FirstOrDefault()?.Url;
            var story = string.IsNullOrWhiteSpace(a.Story) ? "" : a.Story.Trim();

            html += $@"

    <!-- ARTICLE CARD -->
    <div style='
        border:1px solid #ddd;
        border-radius:10px;
        padding:16px;
        margin-bottom:22px;
        background:#fafafa;
    '>

        <!-- Article Header -->
        <div style='display:flex; gap:14px;'>

            {(img != null ?
                    $@"<img src='{img}' 
                    style='width:120px; height:120px; object-fit:cover; border-radius:8px; margin-right: 16px;'/>"
                    : "")}

            <div style='flex:1;'>
                <div style='font-size:14px; color:#888;'>{date}</div>

                <h3 style='
                    margin:6px 0 4px;
                    font-size:20px;
                    color:#111;
                '>
                    {a.Headline}
                </h3>

                {(string.IsNullOrWhiteSpace(a.Description) ? "" :
                        $"<p style='margin:4px 0; color:#555; font-style:italic;'>{a.Description}</p>"
                    )}
            </div>
        </div>

        <!-- STORY BLOCK -->
        {(string.IsNullOrWhiteSpace(story) || !string.IsNullOrWhiteSpace(a?.Links?.Web?.Href) ? "" :
                $@"<div style='
                    margin-top:12px;
                    padding:10px 12px;
                    background:white;
                    border-radius:6px;
                    font-size:14px;
                    color:#444;
                    border:1px solid #eee;
                    white-space:pre-wrap;
                '>
                    {story}
                </div>"
            )}

        <!-- LINK OR MESSAGE -->
        <div style='margin-top:12px;'>
            {(a.Links?.Web?.Href != null
                    ? $"<a href='{a.Links.Web.Href}' style='font-weight:bold; color:#2c7be5;'>Read full article online</a>"
                    : "<span style='font-size:13px; color:#777;'>No online link available — full article above.</span>"
                )}
        </div>

    </div>
";
        }

        html += "</div>";
        return html;
    }

}
