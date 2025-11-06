using OpenAI.Chat;

namespace FFOracle.Services;

public class ChatGPTService
{
    private readonly ChatClient _chatClient;

    public ChatGPTService(IConfiguration configuration)
    {
        var openAIApiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(openAIApiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is missing!");
        }

        _chatClient = new ChatClient("gpt-4", openAIApiKey);
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }

        var completion = await _chatClient.CompleteChatAsync(message);
        return completion.Value.Content[0].Text;
    }
}
