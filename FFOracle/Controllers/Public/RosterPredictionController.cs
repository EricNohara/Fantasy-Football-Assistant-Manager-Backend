using FFOracle.DTOs;
using FFOracle.DTOs.UserLeagueDataRpc;
using FFOracle.Models.Supabase;
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
    private static readonly string[] FLEX_POSITIONS = new[] { "RB", "WR", "TE" };

    private readonly Client _supabase;
    private readonly ChatGPTService _chatService;
    private readonly SupabaseAuthService _authService;

    public RosterPredictionController(Client supabase, ChatGPTService chatService, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _chatService = chatService;
        _authService = authService;
    }

    private int GetStartCountForPosition(string position, RosterSettingsDto rosterSettings)
    {
        int startCount = position switch
        {
            "QB" => rosterSettings.QbCount,
            "RB" => rosterSettings.RbCount,
            "WR" => rosterSettings.WrCount,
            "TE" => rosterSettings.TeCount,
            "FLEX" => rosterSettings.FlexCount,
            "K" => rosterSettings.KCount,
            "DEF" => rosterSettings.DefCount,
            _ => 1
        };

        return startCount;
    }

    // helper function for creating new prompt
    private string BuildAiPrompt(string position, object rosterData, RosterSettingsDto rosterSettings, ScoringSettingsDto scoringSettings)
    {
        int startCount = GetStartCountForPosition(position, rosterSettings);

        return $@"
            You are a fantasy football manager deciding which players to start or sit.

            ### Rules:
            - You can ONLY start **{startCount} {position}{(startCount > 1 ? "s" : "")}**.
            - Consider:
              - Season stats
              - Recent weekly performance trends
              - Opponent defensive matchup
              - Betting odds (spread, implied totals, game script)
              - Game environment (indoor/outdoor, weather if applicable)

            ### Scoring Settings:
            {JsonSerializer.Serialize(scoringSettings)}

            ### Available Players for {position}:
            {JsonSerializer.Serialize(rosterData)}

            ### Output Format (Strict):
            For **each player**, output an object in this exact format:

            {{
              ""Position"": ""QB/RB/WR/TE/FLEX/K/DEF"",
              ""PlayerId"": ""player_id"",
              ""Picked"": true/false,
              ""Reasoning"": ""Explain why the player is or is not picked. Include player stats, recent trends, matchup strength, betting lines, and expected game environment.""
            }}

            ### Output Rules:
            - Output a **JSON array only**.
            - **No extra text** before or after the array.
            - Do **not modify the keys or structure**.

            Begin:
        ".Trim();
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

            // parse roster settings
            var rosterSettings = leagueData.RosterSettings;

            // parse scoring settings
            var scoringSettings = leagueData.ScoringSettings;

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

            var defList = leagueData.Defenses?
                .Select(d =>
                {
                    var json = JsonSerializer.Serialize(
                        d,
                        new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }
                    );

                    return JsonSerializer.Deserialize<DefenseDto>(json);
                })
                .ToList() ?? new List<DefenseDto>();

            // aggregate pruned data by position
            var positionMap = new Dictionary<string, IEnumerable<IPlayerData>>
            {
                { "QB", qbList },
                { "RB", rbList },
                { "WR", wrList },
                { "TE", teList },
                { "K", kList }
            };

            // recommendations to be filled
            var aiRosterRecommendation = new AiRosterRecommendation();

            // for each offensive position group, generate AI predictions
            foreach (var kvp in positionMap)
            {
                var position = kvp.Key;
                var rosterDataForPosition = kvp.Value;
                int startCount = GetStartCountForPosition(position, rosterSettings);
                int positionCount = rosterDataForPosition.Count();

                if (positionCount == 0)
                {
                    // skip if there are no players at this position
                    continue;
                } else if (positionCount <= startCount)
                {
                    // by default the players on the inputted roster must start
                    foreach (var rosterData in rosterDataForPosition)
                    {
                        var defaultRecommendation = new AiPositionRecommendation
                        {
                            Position = position,
                            PlayerId = rosterData.Player.Id,
                            Picked = true,
                            Reasoning = $"{rosterData.Player.Name} is one of {positionCount} total {position}s on your roster in a league that allows {startCount} {position}s. By default, this means to maximize your score for this week, {rosterData.Player.Name} is a must start.{(positionCount < startCount ? $" Strongly consider adding {startCount - positionCount} more {position}s to maximize your roster." : "")}"
                        };
                        aiRosterRecommendation.Recommendations.Add(defaultRecommendation);
                    }
                    continue;
                }

                // create query
                string query = BuildAiPrompt(
                    position,
                    rosterDataForPosition,
                    rosterSettings,
                    scoringSettings
                );

                var aiResponse = await _chatService.SendMessageAsync(query);

                // deserialize recommendation and add it to final output
                var recommendations = JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
                    aiResponse, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // validate and add recommendations
                foreach (var recommendation in recommendations ?? [])
                {
                    if (recommendation != null)
                    {
                        aiRosterRecommendation.Recommendations.Add(recommendation);
                    }
                }
            }

            // generate AI predictions for FLEX (RB/WR/TE that have not already been picked)
            var pickedIds = aiRosterRecommendation.Recommendations
                .Where(r => FLEX_POSITIONS.Contains(r.Position) && r.Picked)
                .Select(r => r.PlayerId)
                .ToHashSet();

            // build a combined list of RB/WR/TE from original roster
            var flexPool = new List<IPlayerData>();
            flexPool.AddRange(rbList);
            flexPool.AddRange(wrList);
            flexPool.AddRange(teList);

            // filter out already picked players
            var flexCandidates = flexPool
                .Where(p => !pickedIds.Contains(p.Player.Id))
                .ToList();

            if (flexCandidates.Any())
            {
                if (flexCandidates.Count <= rosterSettings.FlexCount)
                {
                    // by default the flex candidates on the inputted roster must start
                    foreach (var flexData in flexCandidates)
                    {
                        var defaultFlexRecommendation = new AiPositionRecommendation
                        {
                            Position = "FLEX",
                            PlayerId = flexData.Player.Id ?? "",
                            Picked = true,
                            Reasoning = $"{flexData.Player.Name} is one of {flexCandidates.Count} total FLEX candidates on your roster in a league that allows {rosterSettings.FlexCount} FLEX players. By default, this means to maximize your score for this week, {flexData.Player.Name} is a must start.{(flexCandidates.Count < rosterSettings.FlexCount ? $" Strongly consider adding {rosterSettings.FlexCount - flexCandidates.Count} more FLEX candidates to maximize your roster." : "")}"
                        };
                        aiRosterRecommendation.Recommendations.Add(defaultFlexRecommendation);
                    }
                } else
                {
                    string flexQuery = BuildAiPrompt(
                        "FLEX",
                        flexCandidates,
                        rosterSettings,
                        scoringSettings
                    );

                    var flexResponse = await _chatService.SendMessageAsync(flexQuery);

                    var flexRecs = JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
                        flexResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    // validate and add recommendations
                    foreach (var rec in flexRecs ?? [])
                    {
                        rec.Position = "FLEX";
                        aiRosterRecommendation.Recommendations.Add(rec);
                    }
                }
            }

            // generate AI predictions for DEF
            if (defList.Count > 0)
            {
                if (defList.Count <= rosterSettings.DefCount)
                {
                    // by default the defenses on the inputted roster must start
                    foreach (var defData in defList)
                    {
                        var defaultDefRecommendation = new AiPositionRecommendation
                        {
                            Position = "DEF",
                            PlayerId = defData.Team.Id,
                            Picked = true,
                            Reasoning = $"{defData.Team.Name} is one of {defList.Count} total DEFs on your roster in a league that allows {rosterSettings.DefCount} DEFs. By default, this means to maximize your score for this week, {defData.Team.Name} is a must start.{(defList.Count < rosterSettings.DefCount ? $" Strongly consider adding {rosterSettings.DefCount - defList.Count} more DEFs to maximize your roster." : "")}"
                        };
                        aiRosterRecommendation.Recommendations.Add(defaultDefRecommendation);
                    }
                } else
                {
                    string defQuery = BuildAiPrompt(
                        "DEF",
                        defList,
                        rosterSettings,
                        scoringSettings
                    );

                    var aiDefResponse = await _chatService.SendMessageAsync(defQuery);

                    // deserialize recommendation and add it to final output
                    var defRecommendations = JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
                        aiDefResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    // validate and add recommendations
                    foreach (var recommendation in defRecommendations ?? [])
                    {
                        if (recommendation != null)
                        {
                            aiRosterRecommendation.Recommendations.Add(recommendation);
                        }
                    }
                }
            }

            return Ok(aiRosterRecommendation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating AI predictions: {ex.Message}");
        }
    }
}
