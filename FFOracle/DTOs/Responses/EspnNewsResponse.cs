namespace FFOracle.DTOs.Responses;

//A collection of data sets representing news articles
public class EspnNewsResponse
{
    public List<EspnNewsItem> Feed { get; set; } = new();
}

//Data collection representing a news article about a player
public class EspnNewsItem
{
    public string? Headline { get; set; }
    public string? Description { get; set; }
    public DateTime? Published { get; set; }
    public List<EspnImage> Images { get; set; } = new();
    public string? Story { get; set; }
    public EspnNewsLinks? Links { get; set; }
    public long? PlayerId { get; set; }
    public string? LocalPlayerId { get; set; }
}

//Data to represent an image from ESPN
public class EspnImage
{
    public string? Url { get; set; }
    public string? Alt { get; set; }
}

//A container for a link to a news article
public class EspnNewsLinks
{
    public EspnNewsWebLink? Web { get; set; }
}

//A link to a news article
public class EspnNewsWebLink
{
    public string? Href { get; set; }
}
