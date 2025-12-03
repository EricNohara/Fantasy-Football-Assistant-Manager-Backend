namespace FFOracle.DTOs.Requests;

//Data for creating a new league; includes league info and league settings info
public class CreateLeagueRequest
{
    public string Name { get; set; }
    public int QbCount { get; set; }
    public int RbCount { get; set; }
    public int WrCount { get; set; }
    public int TeCount { get; set; }
    public int FlexCount { get; set; }
    public int KCount { get; set; }
    public int DefenseCount { get; set; }
    public int BenchCount { get; set; }
    public double PointsPerTd { get; set; }
    public double PointsPerReception { get; set; }
    public double PointsPerPassingYard { get; set; }
    public double PointsPerRushingYard { get; set; }
    public double PointsPerReceivingYard { get; set; }
}
