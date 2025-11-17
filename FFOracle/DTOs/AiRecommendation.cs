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