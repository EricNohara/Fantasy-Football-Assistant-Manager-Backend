using Fantasy_Football_Assistant_Manager.DTOs;
using Fantasy_Football_Assistant_Manager.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Gotrue;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController: ControllerBase
{
    private readonly Supabase.Client supabase;

    public UsersController(Supabase.Client supabase)
    {
        this.supabase = supabase;
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
            var session = await supabase.Auth.SignUp(req.Email, req.Password);

            if (session?.User == null || string.IsNullOrEmpty(session.User.Id))
            {
                return StatusCode(500, "Supabase returned an invalid user ID");
            }

            var userId = Guid.Parse(session.User.Id);

            // Insert the new user and their information into users table
            var user = new Models.User
            {
                Id = userId,
                Email = req.Email,
                TeamName = req.TeamName
            };

            await supabase.From<Models.User>().Insert(user);

            return Ok(new
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
            });
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
}
