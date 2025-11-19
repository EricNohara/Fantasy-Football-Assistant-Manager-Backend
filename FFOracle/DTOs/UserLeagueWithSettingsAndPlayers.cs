namespace FFOracle.DTOs;

public class UserLeagueWithSettingsAndPlayers
{
    public UserLeague League { get; set; }
    public RosterSetting RosterSetting { get; set; }
    public ScoringSetting ScoringSetting { get; set; }
    public List<UserLeagueMember> Players { get; set; }
}
