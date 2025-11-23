using OpenAI.Chat;

namespace FFOracle.Services;

public class ChatGPTService
{
    private readonly IConfiguration _config;
    private readonly ChatClient _chatClient;

    public ChatGPTService(IConfiguration configuration)
    {
        _config = configuration;
        var openAIApiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(openAIApiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is missing!");
        }

        _chatClient = new ChatClient("gpt-4", openAIApiKey);
    }

    public async Task<string> SendMessageAsync(string message, string model = "gpt-4")
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }
        //By default, we use gpt-4. If this is the desired model, use the stored client.
        if (model.Equals("gpt-4"))
        {
            var completion = await _chatClient.CompleteChatAsync(message);
            return completion.Value.Content[0].Text;
        }
        //else, make a temporary new client to use for the new model
        else
        {
            var tempClient = new ChatClient(model, _config["OpenAI:ApiKey"]);
            var completion = await tempClient.CompleteChatAsync(message);
            return completion.Value.Content[0].Text;
        }
    }
}
