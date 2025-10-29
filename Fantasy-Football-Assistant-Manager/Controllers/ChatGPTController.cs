using Microsoft.AspNetCore.Mvc;
using Fantasy_Football_Assistant_Manager.Services;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatGPTController : ControllerBase
{
    private readonly ChatGPTService _chatService;

    // constructor - sets up the chatgpt client with api key
    public ChatGPTController(ChatGPTService chatService)
    {
        _chatService = chatService;
    }

    // test endpoint to verify openai api connection
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] string? message = null)
    {
        var testMessage = message ?? "Say 'Hello, I am working!' if you can read this.";

        try
        {
            var response = await _chatService.SendMessageAsync(testMessage);
            return Ok(new
            {
                Success = true,
                Response = response,
                Model = "gpt-4"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error calling OpenAI API: {ex.Message}");
        }
    }
}