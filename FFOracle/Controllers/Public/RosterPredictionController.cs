using FFOracle.DTOs.UserLeagueDataRpc;
using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Square;
using Supabase;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class RosterPredictionController: ControllerBase
{
    private readonly Client _supabase;
    private readonly ChatGPTService _chatService;
    private readonly SupabaseAuthService _authService;

    public RosterPredictionController(Client supabase, ChatGPTService chatService, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _chatService = chatService;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPrediction()
    {
        try
        {
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized("Invalid token");
            }

            // get the inputted league id from query parameters
            var leagueIdStr = HttpContext.Request.Query["leagueId"].ToString();
            if (!Guid.TryParse(leagueIdStr, out Guid leagueId))
            {
                return BadRequest("Invalid leagueId");
            }

            // retrieve user's roster, league settings, and scoring settings for the league
            var getUserLeagueDataRes = await _supabase
                .Rpc("get_user_league_data", new { _user_id = userId, _league_id = leagueId });

            // Assume the RPC returns a string in getUserLeagueDataRes.Models[0] (check your SDK)
            string jsonString = getUserLeagueDataRes.Content.ToString(); // or .Data[0]

            // Deserialize directly as DTO
            var leagueData = JsonSerializer.Deserialize<UserLeagueDataRpcResult>(jsonString);
            if (leagueData == null)
            {
                throw new Exception("User league data is null");
            }

            // split up roster by position
            var roster = leagueData.Players;

            // prune unneeded data by position
            var qbList = roster
                .Where(p => p.Player.Position == "QB")
                .Select(p =>
                {
                    // Create a shallow copy and let JSON ignore nulls
                    var json = JsonSerializer.Serialize(p, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                    return JsonSerializer.Deserialize<UserLeagueQBData>(json);
                })
                .ToList();

            var rbList = roster
                .Where(p => p.Player.Position == "RB")
                .Select(p =>
                {
                    // Create a shallow copy and let JSON ignore nulls
                    var json = JsonSerializer.Serialize(p, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                    return JsonSerializer.Deserialize<UserLeagueFlexData>(json);
                })
                .ToList();

            var wrList = roster
                .Where(p => p.Player.Position == "WR")
                .Select(p =>
                {
                    // Create a shallow copy and let JSON ignore nulls
                    var json = JsonSerializer.Serialize(p, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                    return JsonSerializer.Deserialize<UserLeagueFlexData>(json);
                })
                .ToList();

            var teList = roster
                .Where(p => p.Player.Position == "TE")
                .Select(p =>
                {
                    // Create a shallow copy and let JSON ignore nulls
                    var json = JsonSerializer.Serialize(p, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                    return JsonSerializer.Deserialize<UserLeagueFlexData>(json);
                })
                .ToList();

            var kList = roster
                .Where(p => p.Player.Position == "K")
                .Select(p =>
                {
                    // Create a shallow copy and let JSON ignore nulls
                    var json = JsonSerializer.Serialize(p, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                    return JsonSerializer.Deserialize<UserLeagueKData>(json);
                })
                .ToList();

            // aggregate pruned data by position
            var rosterByPosition = new
            {
                Qb = qbList,
                Rb = rbList,
                Wr = wrList,
                Te = teList,
                K = kList,
            };

            // return for now - testing
            return Ok(rosterByPosition);

            // for each position group, generate AI predictions

            // aggregate all predictions into a single response object

            //return Ok("AI-generated roster predictions");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating AI predictions: {ex.Message}");
        }
    }
}
