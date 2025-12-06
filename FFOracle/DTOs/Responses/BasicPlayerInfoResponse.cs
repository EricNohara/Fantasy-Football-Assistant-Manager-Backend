using System.Text.Json.Serialization;

namespace FFOracle.DTOs.Responses;

public class BasicPlayerInfoResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("headshot_url")]
    public string? HeadshotUrl { get; set; }
}
