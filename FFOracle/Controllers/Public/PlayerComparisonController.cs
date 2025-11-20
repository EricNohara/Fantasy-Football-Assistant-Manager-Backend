using FFOracle.DTOs;
using FFOracle.DTOs.UserLeagueDataRpc;
using FFOracle.Models.Supabase;
using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Square;
using Supabase;
using Supabase.Gotrue;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class PlayerComparisonController : ControllerBase
{
    private static readonly string[] FLEX_POSITIONS = new[] { "RB", "WR", "TE" };

    private readonly Supabase.Client _supabase;
    private readonly ChatGPTService _chatService;
    private readonly SupabaseAuthService _authService;

    public PlayerComparisonController(Supabase.Client supabase, ChatGPTService chatService, SupabaseAuthService authService)
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

        var query = BuildAiPrompt(position, player1, player2, scoringSettings);
        var aiResponse = await _chatService.SendMessageAsync(query);

        return JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
            aiResponse,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? [];
    }

    [HttpGet("{inputId1}/{inputId2}/{position}")]
    public async Task<IActionResult> GetPrediction(
        string inputId1, 
        string inputId2,
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

            //Start by determining if this is a player comparison, a team comparison,
            // or an invalid comparison (player x team or nonexistent player/team).
            //If the comparison is valid, either call the player comparison methor or
            // the team comparison method based on the id type given.
            var id1TypeRes = await _supabase
                .Rpc("is_offensePlayer_or_defenseTeam", new { input_id = inputId1 });
            var id1TypeDTO = JsonSerializer.Deserialize<MemberIdType>(id1TypeRes.Content.ToString());
            int id1Type = id1TypeDTO.Type;

            var id2TypeRes = await _supabase
                .Rpc("is_offensePlayer_or_defenseTeam", new { input_id = inputId2 });
            var id2TypeDTO = JsonSerializer.Deserialize<MemberIdType>(id2TypeRes.Content.ToString());
            int id2Type = id2TypeDTO.Type;

            if (id1Type != id2Type) //if the two id types are not the same, do not compare
            {
                var result = new AiRosterRecommendation();
                result.Recommendations = new List<AiPositionRecommendation>()
                {
                    new AiPositionRecommendation
                    {
                        Position = position,
                        PlayerId = inputId1,
                        Picked = false,
                        Reasoning = $"You've attempted to compare this player to another player/team of a different position. Try comparing two players/teams that can occupy the same position on your roster."
                    }
                };
                return Ok(result);
            }
            else if (id1Type == 0)  //if either the first or second id is unrecognized, do not compare
            {
                var result = new AiRosterRecommendation();
                result.Recommendations = new List<AiPositionRecommendation>()
                {
                    new AiPositionRecommendation
                    {
                        Position = position,
                        PlayerId = inputId1,
                        Picked = false,
                        Reasoning = $"This player either could not be found in our database or cannot be picked for your roster."
                    }
                };
                return Ok(result);
            }
            else if (id2Type == 0)
            {
                var result = new AiRosterRecommendation();
                result.Recommendations = new List<AiPositionRecommendation>()
                {
                    new AiPositionRecommendation
                    {
                        Position = position,
                        PlayerId = inputId2,
                        Picked = false,
                        Reasoning = $"This player either could not be found in our database or cannot be picked for your roster."
                    }
                };
                return Ok(result);
            }
            else if (id1Type == id2Type && id1Type == 1 && position.Equals("DEF"))  //if the request tries to compare players for defense, do not compare
            {
                var result = new AiRosterRecommendation();
                result.Recommendations = new List<AiPositionRecommendation>()
                {
                    new AiPositionRecommendation
                    {
                        Position = position,
                        PlayerId = inputId1,
                        Picked = false,
                        Reasoning = $"You have selected players to compare for a defense position, but players cannot hold defense positions."
                    }
                };
                return Ok(result);
            }
            else if (id1Type == id2Type && id1Type == 2 && !position.Equals("DEF"))
            {
                var result = new AiRosterRecommendation();
                result.Recommendations = new List<AiPositionRecommendation>()
                {
                    new AiPositionRecommendation
                    {
                        Position = position,
                        PlayerId = inputId1,
                        Picked = false,
                        Reasoning = $"You have selected teams to compare for an offense position, but teams cannot hold offense positions."
                    }
                };
                return Ok(result);
            }
            else if (id1Type == 1 && id2Type == 1)
            {
                var ret = await GetPlayerPrediction(inputId1, inputId2, position, leagueId, userId);
                return Ok(ret);
            }
            else if (id1Type == 2 && id2Type == 2)
            {
                var ret = await GetTeamPrediction(inputId1, inputId2, leagueId, userId);
                return Ok(ret);
            }
            else
            {
                throw new Exception($"Error processing player/team ids {inputId1} and {inputId2}");
            }
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating AI predictions: {ex.Message}");
        }
    }

    //handles getting prediction info for offense players
    [NonAction]
    public async Task<AiRosterRecommendation> GetPlayerPrediction(
        string playerId1,
        string playerId2,
        string position,
        Guid leagueId,
        Guid? userId
        )
    {
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
        if (player1Data == null)
        {
            throw new Exception("First player data is null");
        }
        else if (player2Data == null)
        {
            throw new Exception("Second player data is null");
        }

        //Retrieve the league's settings
        var scoringSettingsRes = await _supabase
            .Rpc("get_scoring_settings", new { league_id = leagueId });
        var scoringSettingsString = scoringSettingsRes.Content.ToString();
        var scoringSettings = JsonSerializer.Deserialize<ScoringSettingsDto>(scoringSettingsString);

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

        // decrement the remaining tokens for the user after successful operation ONLY
        await _authService.DecrementUserTokens(userId);

        return aiRosterRecommendation;
    }

    //handles getting prediction info for defense teams
    [NonAction]
    public async Task<AiRosterRecommendation> GetTeamPrediction(
        string teamId1,
        string teamId2,
        Guid leagueId,
        Guid? userId
        )
    {
        //retrieve the data for the two teams being compared
        var getTeam1DataRes = await _supabase
            .Rpc("get_team_data", new { _team_id = teamId1 });
        string team1String = getTeam1DataRes.Content.ToString();

        var getTeam2DataRes = await _supabase
            .Rpc("get_team_data", new { _team_id = teamId2 });
        string team2String = getTeam2DataRes.Content.ToString();

        //deserialize the team data to DTOs
        var team1Data = JsonSerializer.Deserialize<DefenseDto>(team1String);
        var team2Data = JsonSerializer.Deserialize<DefenseDto>(team2String);
        if (team1Data == null)
        {
            throw new Exception("First team data is null");
        }
        else if (team2Data == null)
        {
            throw new Exception("Second team data is null");
        }

        //Retrieve the league's settings
        var scoringSettingsRes = await _supabase
            .Rpc("get_scoring_settings", new { league_id = leagueId });
        var scoringSettingsString = scoringSettingsRes.Content.ToString();
        var scoringSettings = JsonSerializer.Deserialize<ScoringSettingsDto>(scoringSettingsString);

        // recommendations to be filled
        var aiRosterRecommendation = new AiRosterRecommendation();

        // prune unneeded data by position
        List<AiPositionRecommendation> recs = new List<AiPositionRecommendation>();
        DefenseDto cleanT1Data = CloneClean<DefenseDto>(team1Data);
        DefenseDto cleanT2Data = CloneClean<DefenseDto>(team2Data);

        var query = BuildAiPrompt("DEF", cleanT1Data, cleanT2Data, scoringSettings);
        var aiResponse = await _chatService.SendMessageAsync(query);

        aiRosterRecommendation.Recommendations = 
            JsonSerializer.Deserialize<List<AiPositionRecommendation>>(
                aiResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];

        // decrement the remaining tokens for the user after successful operation ONLY
        await _authService.DecrementUserTokens(userId);

        return aiRosterRecommendation;
    }
}
