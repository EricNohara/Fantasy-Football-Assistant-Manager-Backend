namespace Fantasy_Football_Assistant_Manager.DTOs
{
    public class PlayerStatWithWeekNum
    {
        //DTO that holds a season week number and a player stat DTO.
        //Needed to properly identify/order weekly player stats in a list.
        public int Week {  get; set; }
        public DTOs.PlayerStat Stat { get; set; }
    }
}
