//using Fantasy_Football_Assistant_Manager.DTOs;
using FFOracle.Models.Supabase;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Responses;
using System.Reflection;
using System.Text.Json;
using static Supabase.Postgrest.Constants;

//A controller to retrieve all data specific to a certain user
//Based on the SupeBaseController code used for our test app

namespace FFOracle.Controllers.SupabaseControllers;

[ApiController]
[Route("api/[controller]")]
public class GetUserDataController : ControllerBase
{
    private readonly Client _supabase;

    public GetUserDataController(Client supabase)
    {
        _supabase = supabase;
    }

    // Read-only endpoint to fetch all user data: account info, settings, team
    [HttpGet]
    public async Task<IActionResult> GetUserData()
    {
        try
        {
            // get the token from the Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Missing or invalid token");
            }
            var accessToken = authHeader.Substring("Bearer ".Length);
            // verify the token with Supabase
            var user = await _supabase.Auth.GetUser(accessToken);
            if (user == null)
            {
                return Unauthorized("Invalid token");
            }
            var userId = Guid.Parse(user.Id);
            if (userId == Guid.Empty)
            {
                return Unauthorized("Invalid token");
            }

            //get the RPC result and extract the content field as a string
            var result = await _supabase.Rpc("get_user_data", new { _user_id = userId });
            //parse the content field to JSON
            using var doc = JsonDocument.Parse(result.Content.ToString());
            var root = doc.RootElement;
            //pretty-print the result
            var prettyDoc = JsonSerializer.Serialize(root, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return Ok(prettyDoc);
        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}