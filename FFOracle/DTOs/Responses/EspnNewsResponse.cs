namespace FFOracle.DTOs.Responses;

public class EspnNewsResponse
{
    public List<EspnNewsItem> Feed { get; set; } = new();
}

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

public class EspnImage
{
    public string? Url { get; set; }
    public string? Alt { get; set; }
}

public class EspnNewsLinks
{
    public EspnNewsWebLink? Mobile { get; set; }
}

public class EspnNewsWebLink
{
    public string? Href { get; set; }
}
