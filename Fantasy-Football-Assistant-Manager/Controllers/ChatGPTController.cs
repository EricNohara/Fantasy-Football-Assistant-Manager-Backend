using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatGPTController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ChatClient _chatClient;

    // constructor - sets up the chatgpt client with api key
    public ChatGPTController(IConfiguration configuration)
    {
        _configuration = configuration;

        // get the api key from config
        var openAIApiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(openAIApiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is missing! Check appsettings.json or environment variables.");
        }

        // initialize the chat client
        _chatClient = new ChatClient("gpt-4", openAIApiKey);
    }

    // test endpoint to verify openai api connection
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] string? message = null)
    {
        try
        {
            // use custom message or default test message
            var testMessage = message ?? "Say 'Hello, I am working!' if you can read this.";

            // send message to chatgpt
            var completion = await _chatClient.CompleteChatAsync(testMessage);

            // return the response
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

    // chat endpoint for general chatgpt interactions
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        // make sure message isn't empty
        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        try
        {
            // get chatgpt response
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

// simple dto for chat requests
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}
