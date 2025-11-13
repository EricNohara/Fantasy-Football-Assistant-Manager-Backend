namespace FFOracle.DTOs;

public class TeamWithStats
{
    public required Team team {  get; set; }   //team info

    public required TeamDefensiveStat defStat { get; set; }    //defensive stats
    public required TeamOffensiveStat offStat { get; set; }

}
