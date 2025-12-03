using System.Text.Json.Serialization;

namespace FFOracle.DTOs.Responses;

//Data for retrieving player info from API
public class EspnAthleteDetailResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";

    [JsonPropertyName("position")]
    public EspnPosition? Position { get; set; }
}

//Data to represent a player position as retrieved from the API
public class EspnPosition
{
    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; set; } = "";
}
