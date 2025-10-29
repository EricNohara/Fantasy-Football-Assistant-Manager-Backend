namespace Fantasy_Football_Assistant_Manager.Models;

public class TeamSeasonStatCsv
{
    public string Team { get; set; } = String.Empty;

    // DEFENSIVE STATS
    public int? TacklesForLoss { get; set; }
    public int? TacklesForLossYards { get; set; }
    public int? FumblesFor { get; set; }
    public int? SacksFor { get; set; }
    public int? SackYardsFor { get; set; }
    public int? InterceptionsFor { get; set; }
    public int? InterceptionYardsFor { get; set; }
    public int? DefTds { get; set; }
    public int? Safeties { get; set; }
    public int? PassDefended { get; set; }

    // OFFENSIVE STATS
    public int? Completions { get; set; }
    public int? Attempts { get; set; }
    public int? PassingYards { get; set; }
    public int? PassingTds { get; set; }
    public int? PassingInterceptions { get; set; }
    public int? SacksAgainst { get; set; }
    public int? FumblesAgainst { get; set; }
    public int? Carries { get; set; }
    public int? RushingYards { get; set; }
    public int? RushingTds { get; set; }
    public int? Receptions { get; set; }
    public int? Targets { get; set; }
    public int? ReceivingYards { get; set; }
    public int? ReceivingTds { get; set; }
}
