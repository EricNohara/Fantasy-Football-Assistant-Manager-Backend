namespace Fantasy_Football_Assistant_Manager.DTOs;

public class PlayerStat
{
    // DTO object with properties for player stats used in conversion from supabase model
    // temporary DTO since we only need it for the mini research project serializing the output as JSON.
    public int? Completions { get; set; }
    public int? PassingAttempts { get; set; }
    public int? PassingYards { get; set; }
    public int? PassingTds { get; set; }
    public int? InterceptionsAgainst { get; set; }
    public int? SacksAgainst { get; set; }
    public int? FumblesAgainst { get; set; }
    public int? PassingFirstDowns { get; set; }
    public float? PassingEpa { get; set; }
    public int? Carries { get; set; }
    public int? RushingYards { get; set; }
    public int? RushingTds { get; set; }
    public int? RushingFirstDowns { get; set; }
    public float? RushingEpa { get; set; }
    public int? Receptions { get; set; }
    public int? Targets { get; set; }
    public int? ReceivingYards { get; set; }
    public int? ReceivingTds { get; set; }
    public int? ReceivingFirstDowns { get; set; }
    public float? ReceivingEpa { get; set; }
    public List<int>? FgMadeList { get; set; }
    public List<int>? FgMissedList { get; set; }
    public List<int>? FgBlockedList { get; set; }
    public int? PadAttempts { get; set; }
    public float? PatPercent { get; set; }
    public float? FantasyPoints { get; set; }
    public float? FantasyPointsPpr { get; set; }
}
