using System.Text.Json.Serialization;

namespace FFOracle.DTOs.Responses;

public class EspnAthleteDetailResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";

    [JsonPropertyName("position")]
    public EspnPosition? Position { get; set; }
}

public class EspnPosition
{
    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; set; } = "";
}
