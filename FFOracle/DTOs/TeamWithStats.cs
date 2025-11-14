namespace FFOracle.DTOs
{
    public class TeamWithStats
    {
        public required DTOs.Team team {  get; set; }   //team info

        public required DTOs.TeamDefensiveStat defStat { get; set; }    //defensive stats
        public required DTOs.TeamOffensiveStat offStat { get; set; }

    }
}
