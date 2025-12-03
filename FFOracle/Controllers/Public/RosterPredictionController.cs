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
    private const int TOKENS_PER_REQUEST = 10;

    private readonly Client _supabase;
    private readonly ChatGPTService _chatService;
    private readonly SupabaseAuthService _authService;

    public RosterPredictionController(Client supabase, ChatGPTService chatService, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _chatService = chatService;
        _authService = authService;
    }

    // helper for cloning player data while cleaning fields by their position
    private static T? CloneClean<T>(object source)
    {
        var json = JsonSerializer.Serialize(source, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        return JsonSerializer.Deserialize<T>(json);
    }

    // helper function for getting start count by position
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
            - Maximize projected points for the upcoming week
            - You MUST start exactly {startCount} players for this position unless there are fewer than {startCount} total players available.
            - If there are fewer than {startCount} total players available, you MUST start all available players.
            - If there are more than {startCount} total players available, you MUST choose which players to start and which to sit.
            - If a player has NO game data or opponent or is injured, they should NOT be started unless there are insufficient healthy players to fill the position.
            - Consider:
              - Season stats
              - Recent weekly performance trends
              - Opponent defensive matchup
              - Betting odds (spread, implied totals, game script)
              - Game environment (indoor/outdoor, home/away, divisional?)

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

    // helper for generating prediction for players
    private async Task<List<AiPositionRecommendation>> GeneratePositionRecommendations<T>(
        string position,
        IEnumerable<T> rosterData,
        RosterSettingsDto rosterSettings,
        ScoringSettingsDto scoringSettings
    ) where T : IPlayerData
    {
        var list = rosterData.ToList();
        int startCount = GetStartCountForPosition(position, rosterSettings);

        if (list.Count <= startCount)
        {
            return list.Select(p => new AiPositionRecommendation
            {
                Position = position,
                PlayerId = p.Player.Id,
                Picked = true,
                Reasoning = $"{p.Player.Name} is one of {list.Count} total {position}s on your roster in a league that allows {startCount} {position}s. By default, this means to maximize your score for this week, {p.Player.Name} is a must start.{(list.Count < startCount ? $" Strongly consider adding {startCount - list.Count} more {position}s to maximize your roster." : "")}"
            }).ToList();
        }

        var query = BuildAiPrompt(position, list, rosterSettings, scoringSettings);
        var aiResponse = await _chatService.SendMessageAsync(query);

        return JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
            aiResponse,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? [];
    }

    //route to get team comp prediction
    [HttpGet]
    public async Task<IActionResult> GetPrediction()
    {
        try
        {
            //authenitcate user
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized("Invalid token");
            }

            // get the remaining tokens for the user
            var remainingTokens = await _authService.GetUserTokensLeft(userId);

            // return Unauthorized if no tokens left
            if (remainingTokens <= TOKENS_PER_REQUEST)
            {
                return Unauthorized("No enough tokens remaining");
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
                .Select(CloneClean<UserLeagueQBData>)
                .ToList();

            var rbList = roster
                .Where(p => p.Player.Position == "RB")
                .Select(CloneClean<UserLeagueFlexData>)
                .ToList();

            var wrList = roster
                .Where(p => p.Player.Position == "WR")
                .Select(CloneClean<UserLeagueFlexData>)
                .ToList();

            var teList = roster
                .Where(p => p.Player.Position == "TE")
                .Select(CloneClean<UserLeagueFlexData>)
                .ToList();

            var kList = roster
                .Where(p => p.Player.Position == "K")
                .Select(CloneClean<UserLeagueKData>)
                .ToList();

            var defList = leagueData.Defenses?
                .Select(CloneClean<DefenseDto>)
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
                var recs = await GeneratePositionRecommendations(
                    kvp.Key,
                    kvp.Value,
                    rosterSettings,
                    scoringSettings
                );

                aiRosterRecommendation.Recommendations.AddRange(recs);
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
                var flexRecs = await GeneratePositionRecommendations(
                    "FLEX",
                    flexCandidates,
                    rosterSettings,
                    scoringSettings
                );

                aiRosterRecommendation.Recommendations.AddRange(flexRecs);
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

            // decrement the remaining tokens for the user after successful operation ONLY
            await _authService.DecrementUserTokens(userId, TOKENS_PER_REQUEST);

            return Ok(aiRosterRecommendation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating AI predictions: {ex.Message}");
        }
    }
}
