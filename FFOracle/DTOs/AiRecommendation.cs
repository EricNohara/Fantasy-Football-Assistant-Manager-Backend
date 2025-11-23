namespace FFOracle.DTOs;

public class AiPositionRecommendation
{
    public string Position { get; set; }
    public string PlayerId { get; set; }
    public bool Picked { get; set; }
    public string Reasoning { get; set; }
}

public class AiRosterRecommendation
{
    public List<AiPositionRecommendation> Recommendations { get; set; } = new List<AiPositionRecommendation>();
}

public class AiArticle
{
    public string Title { get; set; }
    public string Link { get; set; }
    public string Summary { get; set; }
}

public class AiArticleCollection
{
    public List<AiArticle> Articles { get; set; } = new List<AiArticle>();
}