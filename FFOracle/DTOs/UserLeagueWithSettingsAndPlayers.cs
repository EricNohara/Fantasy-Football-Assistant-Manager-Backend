namespace FFOracle.DTOs;

public class UserLeagueWithSettingsAndPlayers
{
    //information on a league, its settings, and all players in the league
    public UserLeague League { get; set; }
    public RosterSetting RosterSetting { get; set; }
    public ScoringSetting ScoringSetting { get; set; }
    public List<UserLeagueMember> Players { get; set; }
}
