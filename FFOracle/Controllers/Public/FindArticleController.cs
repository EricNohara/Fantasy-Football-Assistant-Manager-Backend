using FFOracle.DTOs;
using FFOracle.DTOs.UserLeagueDataRpc;
using FFOracle.Models.Supabase;
using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Square;
using Supabase;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OpenAI;

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class FindArticleController : ControllerBase
{
    private readonly Client _supabase;
    private readonly ChatGPTService _chatService;
    private readonly SupabaseAuthService _authService;

    public FindArticleController(
        Client supabase, 
        ChatGPTService chatService, 
        SupabaseAuthService authService
        )
    {
        _supabase = supabase;
        _authService = authService;
        _chatService = chatService;
    }

    // helper function for creating new prompt
    private string BuildAiPrompt(object roster)
    {
        return $@"
            You are a fantasy football manager looking for online articles about your players for the upcoming week.
            You want recent articles pertaining to your players that could suggest how they might perform in their next games.

            ### Rules:
            - Find at most one article for each player.
            - Find the most recent articles possible.
            - ONLY select articles published within the last year.
            - Prioritize articles about specific players over articles about entire teams.
            - ONLY use articles that exist online. Do not create new articles or supply false links.

            ### Players:
            {JsonSerializer.Serialize(roster)}

            ### Output Format (Strict):
            For **each player**, output an object in this exact format:

            {{
              ""Title"": ""title"",
              ""Link"": ""article_link"",
              ""Summary"": ""Write a short summary of the contents of the article. Limit this to Four sentences at most."",
            }}

            ### Output Rules:
            - Output a **JSON array only**.
            - **No extra text** before or after the array.
            - Do **not modify the keys or structure**.

            Begin:
        ".Trim();
    }

    [HttpGet]
    public async Task<IActionResult> GetArticles()
    {
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

            // retrieve the user's roster
            var rosterRes = await _supabase
                .Rpc("get_league_players", new { _league_id = leagueId });
            string jsonString = rosterRes.Content.ToString();
            var roster = JsonSerializer.Deserialize<List<DTOs.Player>>(jsonString);
            if(roster == null)
            {
                throw new Exception("Roster is empty");
            }

            //create object to hold articles found
            var aiArticleCollection = new AiArticleCollection();

            //Request articles from AI for roster
            var query = BuildAiPrompt(roster);
            //var aiResponse = await _chatService.SendMessageAsync(query);
            var aiResponse = await _chatService.SendMessageAsync(query, "gpt-4.1");

            var responses = JsonSerializer.Deserialize<List<AiArticle>>(
                aiResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];
            aiArticleCollection.Articles = responses;

            // decrement the remaining tokens for the user after successful operation ONLY
            await _authService.DecrementUserTokens(userId);

            return Ok(aiArticleCollection);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating AI predictions: {ex.Message}");
        }
    }
}
