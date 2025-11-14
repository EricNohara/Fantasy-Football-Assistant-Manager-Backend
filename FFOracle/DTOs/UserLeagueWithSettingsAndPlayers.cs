namespace FFOracle.DTOs
{
    public class UserLeagueWithSettingsAndPlayers
    {
        public DTOs.UserLeague League { get; set; }
        public DTOs.RosterSetting RosterSetting { get; set; }
        public DTOs.ScoringSetting ScoringSetting { get; set; }
        public List<DTOs.UserLeagueMember> Players { get; set; }
    }
}
