namespace FFOracle.DTOs;

//info on an AI's recommendation for whether to pick a player or not
public class AiPositionRecommendation
{
    public string Position { get; set; }
    public string PlayerId { get; set; }
    public bool Picked { get; set; }
    public string Reasoning { get; set; }
}

//A collection of individual player recommendations made by AI
public class AiRosterRecommendation
{
    public List<AiPositionRecommendation> Recommendations { get; set; } = new List<AiPositionRecommendation>();
}