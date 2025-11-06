using Fantasy_Football_Assistant_Manager.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Gotrue;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController: ControllerBase
{
    private readonly Supabase.Client _supabase;
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _serviceRoleKey;

    public UsersController(Supabase.Client supabase, IConfiguration config)
    {
        _supabase = supabase;
        _httpClient = new HttpClient();
        _supabaseUrl = config["Supabase:Url"];
        _serviceRoleKey = config["Supabase:ServiceRoleKey"];
    }

    // SIGN UP
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] CreateUserRequest req)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
        {
            return BadRequest("Cannot create a user without a valid email or password");
        }

        try
        {
            // Sign up user (service role key)
            var session = await _supabase.Auth.SignUp(req.Email, req.Password);

            if (session?.User == null || string.IsNullOrEmpty(session.User.Id))
            {
                return StatusCode(500, "Supabase returned an invalid user ID");
            }

            var userId = Guid.Parse(session.User.Id);

            // Insert the new user and their information into users table
            var user = new Models.Supabase.User
            {
                Id = userId,
                Email = req.Email,
                TeamName = req.TeamName
            };

            await _supabase.From<Models.Supabase.User>().Insert(user);

            return Ok(new { message = "User created successfully" });
        }
        catch (Supabase.Postgrest.Exceptions.PostgrestException e) when (e.Message.Contains("duplicate"))
        {
            return BadRequest("User with this email already exists.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating user: {ex.Message}");
        }
    }

    // DELETE
    [HttpDelete]
    public async Task<IActionResult> Delete()
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

            // delete the user from public db
            await _supabase
                .From<Models.Supabase.User>()
                .Where(u => u.Id == userId)
                .Delete();

            // delete the Supabase Auth user using admin API
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);

            var adminResponse = await _httpClient.DeleteAsync($"{_supabaseUrl}/auth/v1/admin/users/{userId}");

            if (!adminResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)adminResponse.StatusCode, adminResponse.Content.ToString());
            }

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting user: {ex.Message}");
        }
    }
}
