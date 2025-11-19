using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using System.Text.Json;

//A controller to retrieve all data specific to a certain user
//Based on the SupeBaseController code used for our test app

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class GetUserDataController : ControllerBase
{
    private readonly Client _supabase;
    private readonly SupabaseAuthService _authService;

    public GetUserDataController(Client supabase, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _authService = authService;
    }

    // Read-only endpoint to fetch all user data: account info, settings, team
    [HttpGet]
    public async Task<IActionResult> GetUserData()
    {
        try
        {
            // get the token from the Authorization header
            var userId = await _authService.AuthorizeUser(Request);
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