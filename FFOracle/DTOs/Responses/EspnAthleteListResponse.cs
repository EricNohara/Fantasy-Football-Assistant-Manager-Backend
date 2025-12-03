using System.Text.Json.Serialization;

namespace FFOracle.DTOs.Responses;

//Data to represent a portion of a list of athlete info
public class EspnAthleteListResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("pageCount")]
    public int PageCount { get; set; }

    [JsonPropertyName("items")]
    public List<EspnRefItem> Items { get; set; } = new();
}

//Data to represent athlete info pulled from the API
public class EspnRefItem
{
    [JsonPropertyName("$ref")]
    public string Ref { get; set; } = string.Empty;
}