using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatGPTController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ChatClient _chatClient;

    public ChatGPTController(IConfiguration configuration)
    {
        _configuration = configuration;

        var openAIApiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(openAIApiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is missing! Check appsettings.json or environment variables.");
        }

        _chatClient = new ChatClient("gpt-4", openAIApiKey);
    }

    // Test endpoint to verify OpenAI API connection
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] string? message = null)
    {
        try
        {
            var testMessage = message ?? "Say 'Hello, I am working!' if you can read this.";

            var completion = await _chatClient.CompleteChatAsync(testMessage);

            return Ok(new
            {
                Success = true,
                Response = completion.Value.Content[0].Text,
                Model = "gpt-4"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error calling OpenAI API: {ex.Message}");
        }
    }

    // Chat endpoint for general ChatGPT interactions
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        try
        {
            var completion = await _chatClient.CompleteChatAsync(request.Message);

            return Ok(new
            {
                Response = completion.Value.Content[0].Text,
                Model = "gpt-4"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error calling OpenAI API: {ex.Message}");
        }
    }
}

// DTO for chat requests
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}
