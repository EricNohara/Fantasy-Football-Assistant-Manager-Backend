using FFOracle.DTOs;
using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Exceptions;

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class UsersController: ControllerBase
{
    private readonly Client _supabase;
    private readonly SupabaseAuthService _authService;

    public UsersController(Client supabase, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _authService = authService;
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
                Email = req.Email
            };

            await _supabase.From<Models.Supabase.User>().Insert(user);

            return Ok(new { message = "User created successfully" });
        }
        catch (PostgrestException e) when (e.Message.Contains("duplicate"))
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
            var userId = await _authService.AuthorizeUser(Request);
            if (userId == Guid.Empty)
            {
                return Unauthorized("Invalid token");
            }

            // delete the user from public db
            await _supabase
                .From<Models.Supabase.User>()
                .Where(u => u.Id == userId)
                .Delete();

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting user: {ex.Message}");
        }
    }
}
