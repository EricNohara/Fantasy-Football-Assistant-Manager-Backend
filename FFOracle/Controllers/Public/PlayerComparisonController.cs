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
public class PlayerComparisonController : ControllerBase
{
    private static readonly string[] FLEX_POSITIONS = new[] { "RB", "WR", "TE" };

    private readonly Client _supabase;
    private readonly ChatGPTService _chatService;
    private readonly SupabaseAuthService _authService;

    public PlayerComparisonController(Client supabase, ChatGPTService chatService, SupabaseAuthService authService)
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

    //// helper function for getting start count by position
    //private int GetStartCountForPosition(string position, RosterSettingsDto rosterSettings)
    //{
    //    int startCount = position switch
    //    {
    //        "QB" => rosterSettings.QbCount,
    //        "RB" => rosterSettings.RbCount,
    //        "WR" => rosterSettings.WrCount,
    //        "TE" => rosterSettings.TeCount,
    //        "FLEX" => rosterSettings.FlexCount,
    //        "K" => rosterSettings.KCount,
    //        "DEF" => rosterSettings.DefCount,
    //        _ => 1
    //    };

    //    return startCount;
    //}

    // helper function for creating new prompt
    private string BuildAiPrompt(
        string position, 
        object player1,
        object player2,
        ScoringSettingsDto scoringSettings
        ){
        //int startCount = GetStartCountForPosition(position, rosterSettings);

        return $@"
            You are a fantasy football manager deciding which of two players should start and which should sit.

            ### Rules:
            - You can ONLY start one of them.
            - Consider:
              - Season stats
              - Recent weekly performance trends
              - Opponent defensive matchup
              - Betting odds (spread, implied totals, game script)
              - Game environment (indoor/outdoor, weather if applicable)

            ### Scoring Settings:
            {JsonSerializer.Serialize(scoringSettings)}

            ### Players being compared for {position}:
            - {JsonSerializer.Serialize(player1)}
            - {JsonSerializer.Serialize(player2)}

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
        T player1,
        T player2,
        ScoringSettingsDto scoringSettings
    ) where T : IPlayerData
    {
        //var list = players.ToList();
        //int startCount = GetStartCountForPosition(position, rosterSettings);

        //Verify that the players being compared can occupy the same role
        bool normalMatch = player1.Player.Position.Equals(position)
                   && player2.Player.Position.Equals(position);

        bool flexMatch = position.Equals("FLEX")
                         && FLEX_POSITIONS.Contains(player1.Player.Position)
                         && FLEX_POSITIONS.Contains(player2.Player.Position);

        if (!normalMatch && !flexMatch) 
        {
            return new List<AiPositionRecommendation>
            {
                new AiPositionRecommendation
                {
                    Position = player1.Player.Position,
                    PlayerId = player1.Player.Id,
                    Picked = false,
                    Reasoning = $"{player1.Player.Name} and {player2.Player.Name} do not both fit the position of {position} and cannot be properly compared. Try comparing two players elegible for the same position."
                },
                new AiPositionRecommendation
                {
                    Position = player2.Player.Position,
                    PlayerId = player2.Player.Id,
                    Picked = false,
                    Reasoning = $"{player2.Player.Name} and {player1.Player.Name} do not both fit the position of {position} and cannot be properly compared. Try comparing two players elegible for the same position."
                },
            };
        }
        else if(player1.Player.Id.Equals(player2.Player.Id)) //Also check if the same player was selected twice.
        {
            return new List<AiPositionRecommendation>
             {
                 new AiPositionRecommendation
                 {
                     Position = player1.Player.Position,
                     PlayerId = player1.Player.Id,
                     Picked = false,
                     Reasoning = $"You have selected {player1.Player.Id} twice. Try comparing two different players eligible for the same role."
                 }
             };
        }
        
            //if (list.Count <= startCount)
            //{
            //    return list.Select(p => new AiPositionRecommendation
            //    {
            //        Position = position,
            //        PlayerId = p.Player.Id,
            //        Picked = true,
            //        Reasoning = $"{p.Player.Name} is one of {list.Count} total {position}s on your roster in a league that allows {startCount} {position}s. By default, this means to maximize your score for this week, {p.Player.Name} is a must start.{(list.Count < startCount ? $" Strongly consider adding {startCount - list.Count} more {position}s to maximize your roster." : "")}"
            //    }).ToList();
            //}

        var query = BuildAiPrompt(position, player1, player2, scoringSettings);
        var aiResponse = await _chatService.SendMessageAsync(query);

        return JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
            aiResponse,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? [];
    }

    [HttpGet("{playerId1}/{playerId2}/{position}")]
    public async Task<IActionResult> GetPrediction(
        string playerId1, 
        string playerId2,
        string position
        ){
        try
        {
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized("Invalid token");
            }

            // get the remaining tokens for the user
            var remainingTokens = await _authService.GetUserTokensLeft(userId);

            // return Unauthorized if no tokens left
            if (remainingTokens <= 0)
            {
                return Unauthorized("No tokens remaining");
            }

            // get the inputted league id from query parameters
            var leagueIdStr = HttpContext.Request.Query["leagueId"].ToString();
            if (!Guid.TryParse(leagueIdStr, out Guid leagueId))
            {
                return BadRequest("Invalid leagueId");
            }

            //retrieve the data for the two players being compared
            var getPlayer1DataRes = await _supabase
                .Rpc("get_player_data", new { player_id = playerId1 });
            string player1String = getPlayer1DataRes.Content.ToString();

            var getPlayer2DataRes = await _supabase
                .Rpc("get_player_data", new { player_id = playerId2 });
            string player2String = getPlayer2DataRes.Content.ToString();

            //deserialize the player data to DTOs
            var player1Data = JsonSerializer.Deserialize<PlayerDto>(player1String);
            var player2Data = JsonSerializer.Deserialize<PlayerDto>(player2String);
            if(player1Data == null)
            {
                throw new Exception("First player data is null");
            } else if (player2Data == null)
            {
                throw new Exception("Second player data is null");
            }

            //// retrieve user's roster, league settings, and scoring settings for the league
            //var getUserLeagueDataRes = await _supabase
            //    .Rpc("get_user_league_data", new { _user_id = userId, _league_id = leagueId });

            //// Assume the RPC returns a string in getUserLeagueDataRes.Models[0] (check your SDK)
            //string jsonString = getUserLeagueDataRes.Content.ToString(); // or .Data[0]

            //Retrieve the league's settings
            var scoringSettingsRes = await _supabase
                .Rpc("get_scoring_settings", new { league_id = leagueId });
            var scoringSettingsString = scoringSettingsRes.Content.ToString();
            var scoringSettings = JsonSerializer.Deserialize<ScoringSettingsDto>(scoringSettingsString);

            //// parse roster settings
            //var rosterSettings = leagueData.RosterSettings;

            //// parse scoring settings
            //var scoringSettings = leagueData.ScoringSettings;

            //// split up roster by position
            //var roster = leagueData.Players;

            // recommendations to be filled
            var aiRosterRecommendation = new AiRosterRecommendation();

            // prune unneeded data by position
            List<AiPositionRecommendation> recs = new List<AiPositionRecommendation>();
            IPlayerData cleanP1Data = null;
            IPlayerData cleanP2Data = null;
            switch (position)
            {
                case "QB":
                    cleanP1Data = CloneClean<UserLeagueQBData>(player1Data);
                    cleanP2Data = CloneClean<UserLeagueQBData>(player2Data);
                    break;
                case "RB":
                    cleanP1Data = CloneClean<UserLeagueFlexData>(player1Data);
                    cleanP2Data = CloneClean<UserLeagueFlexData>(player2Data);
                    break;
                case "WR":
                    cleanP1Data = CloneClean<UserLeagueFlexData>(player1Data);
                    cleanP2Data = CloneClean<UserLeagueFlexData>(player2Data);
                    break;
                case "TE":
                    cleanP1Data = CloneClean<UserLeagueFlexData>(player1Data);
                    cleanP2Data = CloneClean<UserLeagueFlexData>(player2Data);
                    break;
                case "K":
                    cleanP1Data = CloneClean<UserLeagueKData>(player1Data);
                    cleanP2Data = CloneClean<UserLeagueKData>(player2Data);
                    break;
                case "FLEX":
                    cleanP1Data = CloneClean<UserLeagueFlexData>(player1Data);
                    cleanP2Data = CloneClean<UserLeagueFlexData>(player2Data);
                    break;
            }
            recs = await GeneratePositionRecommendations(position, cleanP1Data, cleanP2Data, scoringSettings);
            aiRosterRecommendation.Recommendations = recs;

            //var qbList = roster
            //    .Where(p => p.Player.Position == "QB")
            //    .Select(CloneClean<UserLeagueQBData>)
            //    .ToList();

            //var rbList = roster
            //    .Where(p => p.Player.Position == "RB")
            //    .Select(CloneClean<UserLeagueFlexData>)
            //    .ToList();

            //var wrList = roster
            //    .Where(p => p.Player.Position == "WR")
            //    .Select(CloneClean<UserLeagueFlexData>)
            //    .ToList();

            //var teList = roster
            //    .Where(p => p.Player.Position == "TE")
            //    .Select(CloneClean<UserLeagueFlexData>)
            //    .ToList();

            //var kList = roster
            //    .Where(p => p.Player.Position == "K")
            //    .Select(CloneClean<UserLeagueKData>)
            //    .ToList();

            //var defList = leagueData.Defenses?
            //    .Select(CloneClean<DefenseDto>)
            //    .ToList() ?? new List<DefenseDto>();

            // aggregate pruned data by position
            //var positionMap = new Dictionary<string, IEnumerable<IPlayerData>>
            //{
            //    { "QB", qbList },
            //    { "RB", rbList },
            //    { "WR", wrList },
            //    { "TE", teList },
            //    { "K", kList }
            //};

            //// recommendations to be filled
            //var aiRosterRecommendation = new AiRosterRecommendation();

            //// for each offensive position group, generate AI predictions
            //foreach (var kvp in positionMap)
            //{
            //    var recs = await GeneratePositionRecommendations(
            //        kvp.Key,
            //        kvp.Value,
            //        rosterSettings,
            //        scoringSettings
            //    );

            //    aiRosterRecommendation.Recommendations.AddRange(recs);
            //}

            //// generate AI predictions for FLEX (RB/WR/TE that have not already been picked)
            //var pickedIds = aiRosterRecommendation.Recommendations
            //    .Where(r => FLEX_POSITIONS.Contains(r.Position) && r.Picked)
            //    .Select(r => r.PlayerId)
            //    .ToHashSet();

            //// build a combined list of RB/WR/TE from original roster
            //var flexPool = new List<IPlayerData>();
            //flexPool.AddRange(rbList);
            //flexPool.AddRange(wrList);
            //flexPool.AddRange(teList);

            //// filter out already picked players
            //var flexCandidates = flexPool
            //    .Where(p => !pickedIds.Contains(p.Player.Id))
            //    .ToList();

            //if (flexCandidates.Any())
            //{
            //    var flexRecs = await GeneratePositionRecommendations(
            //        "FLEX",
            //        flexCandidates,
            //        rosterSettings,
            //        scoringSettings
            //    );

            //    aiRosterRecommendation.Recommendations.AddRange(flexRecs);
            //}

            //// generate AI predictions for DEF
            //if (defList.Count > 0)
            //{
            //    if (defList.Count <= rosterSettings.DefCount)
            //    {
            //        // by default the defenses on the inputted roster must start
            //        foreach (var defData in defList)
            //        {
            //            var defaultDefRecommendation = new AiPositionRecommendation
            //            {
            //                Position = "DEF",
            //                PlayerId = defData.Team.Id,
            //                Picked = true,
            //                Reasoning = $"{defData.Team.Name} is one of {defList.Count} total DEFs on your roster in a league that allows {rosterSettings.DefCount} DEFs. By default, this means to maximize your score for this week, {defData.Team.Name} is a must start.{(defList.Count < rosterSettings.DefCount ? $" Strongly consider adding {rosterSettings.DefCount - defList.Count} more DEFs to maximize your roster." : "")}"
            //            };
            //            aiRosterRecommendation.Recommendations.Add(defaultDefRecommendation);
            //        }
            //    }
            //    else
            //    {
            //        string defQuery = BuildAiPrompt(
            //            "DEF",
            //            defList,
            //            rosterSettings,
            //            scoringSettings
            //        );

            //        var aiDefResponse = await _chatService.SendMessageAsync(defQuery);

            //        // deserialize recommendation and add it to final output
            //        var defRecommendations = JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
            //            aiDefResponse,
            //            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            //        );

            //        // validate and add recommendations
            //        foreach (var recommendation in defRecommendations ?? [])
            //        {
            //            if (recommendation != null)
            //            {
            //                aiRosterRecommendation.Recommendations.Add(recommendation);
            //            }
            //        }
            //    }
            //}

            // decrement the remaining tokens for the user after successful operation ONLY
            await _authService.DecrementUserTokens(userId);

            return Ok(aiRosterRecommendation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating AI predictions: {ex.Message}");
        }
    }
}
