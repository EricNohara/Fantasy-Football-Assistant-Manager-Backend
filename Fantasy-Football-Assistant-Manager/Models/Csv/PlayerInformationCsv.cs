namespace Fantasy_Football_Assistant_Manager.Models.Csv;

public class PlayerInformationCsv
{
    public string Id { get; set; } = string.Empty;
    public int LastSeason { get; set; } = -1;
    public string LatestTeam { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Status { get; set; } = string.Empty;
}
