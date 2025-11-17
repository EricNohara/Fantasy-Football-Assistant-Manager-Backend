using FFOracle.Models.Supabase;
using FFOracle.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Responses;
using static Supabase.Postgrest.Constants;

//A controller to retrieve all data specific to a certain user
//Based on the SupeBaseController code used for our test app

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class GetPlayersByPositionController : ControllerBase
{
    private readonly Client _supabase;

    public GetPlayersByPositionController(Client supabase)
    {
        _supabase = supabase;
    }

    // Read-only endpoint to fetch all players of a certain position.
    // If the position is defense, it instead returns all teams.
    [HttpGet("{position}/{seasonStartYear}")]
    public async Task<IActionResult> GetPlayersByPosition(string position, int seasonStartYear)
    {
        try
        {
            //if the defense has been requested, return a list of teams since defense
            // is per-team
            if (position.Equals("DEF"))
            {
                //get each team
                var teamsRes = await _supabase.From<Team>().Get();
                var teams = teamsRes.Models.ToList();
                //for each team, get its stats and create a new teamWithStats DTO
                var teamWithStatsTasks = teams.Select(async team =>
                {
                    //get the team stats
                    var defRes = await _supabase
                        .From<TeamDefensiveStat>()
                        .Where(s => s.Id == team.DefensiveStatsId)
                        .Get();
                    var offRes = await _supabase
                        .From<TeamOffensiveStat>()
                        .Where(s => s.Id == team.OffensiveStatsId)
                        .Get();
                    var def = defRes.Model;
                    var off = offRes.Model;
                    //Create the full DTO
                    var teamWithStats = new DTOs.TeamWithStats
                    {
                        team = new DTOs.Team
                        {
                            Id = team.Id,
                            Name = team.Name,
                            OffensiveStatsId = team.OffensiveStatsId,
                            DefensiveStatsId = team.DefensiveStatsId
                        },
                        defStat = new DTOs.TeamDefensiveStat
                        {
                            Id = def.Id,
                            TacklesForLoss = def.TacklesForLoss,
                            TacklesForLossYards = def.TacklesForLossYards,
                            FumblesFor = def.FumblesFor,
                            SacksFor = def.SacksFor,
                            SackYardsFor = def.SackYardsFor,
                            InterceptionsFor = def.InterceptionsFor,
                            InterceptionYardsFor = def.InterceptionYardsFor,
                            DefTds = def.DefTds,
                            Safeties = def.Safeties,
                            PassDefended = def.PassDefended
                        },
                        offStat = new DTOs.TeamOffensiveStat
                        {
                            Id = off.Id,
                            Completions = off.Completions,
                            Attempts = off.Attempts,
                            PassingYards = off.PassingYards,
                            PassingTds = off.PassingTds,
                            PassingInterceptions = off.PassingInterceptions,
                            SacksAgainst = off.SacksAgainst,
                            FumblesAgainst = off.FumblesAgainst,
                            Carries = off.Carries,
                            RushingYards = off.RushingYards,
                            RushingTds = off.RushingTds,
                            Receptions = off.Receptions,
                            Targets = off.Targets,
                            ReceivingYards = off.ReceivingYards,
                            ReceivingTds = off.ReceivingTds
                        }
                    };
                    return teamWithStats;
                });
                //since the query that forms the list of DTOs contains multiple async calls,
                // the program needs to wait for everything to complete before it can
                // use the results.
                var teamWithStatsDTOs = (await Task.WhenAll(teamWithStatsTasks)).ToList();

                return Ok(teamWithStatsDTOs);
            }
            //else, return a list of players of that particular position
            else
            {
                //First, get each player of that position joined to their season stats
                //Note about the syntax: the * indicates all fields.
                //Since there is a foreign key for stats in players, I can join the two tables
                // implicitly like this:
                var playersRes = await _supabase
                    .From<Player>()
                    //.Select("*, season_stats(*)") 
                    .Where(p => p.Position == position)
                    .Get();
                var players = playersRes.Models;

                //Second, call a helper method to bind the players' stats to the players
                var playerWithStatsDTOs = await Helper.GetPlayersWithStats(players, _supabase, seasonStartYear);

                ////Second, retrieve all of the player-to-weeklyStats mappings.
                //var playerIds = players.Select(p => p.Id).ToList(); //used to get all mappings associated with pulled players
                //var mappingRes = await _supabase
                //    .From<WeeklyPlayerStat>()
                //    .Where(m => m.SeasonStartYear == seasonStartYear)   //ensure this is from the right season
                //    .Filter(x => x.PlayerId, Operator.In, playerIds)
                //    .Get();
                //var mappings = mappingRes.Models;

                ////Third, use those mappings to retrieve all of the weekly stats.
                //var mappingIds = mappings.Select(m => m.PlayerStatsId).ToList();    //used to get all stats associated with pulled players
                //var statsRes = await _supabase
                //    .From<PlayerStat>()
                //    .Filter(x => x.Id, Operator.In, mappingIds)
                //    .Get();
                //var stats = statsRes.Models;

                ////Fourth, combine each player's weekly stats into a list and use all of the
                //// collected info to form the playerWithStats DTOs.
                //var playerTasks = players.Select(async player =>
                //{
                //    //get this player's season stats
                //    var seasonStatsRes = await _supabase
                //        .From<PlayerStat>()
                //        .Where(s => s.Id == player.SeasonStatsId)
                //        .Get();
                //    var seasonStats = seasonStatsRes.Model;
                //    //get this player's stats mappings, making sure to not select the season stats
                //    var playerMappings = mappings
                //        .Where(m => m.PlayerId == player.Id && m.PlayerStatsId != player.SeasonStatsId)
                //        .ToList();
                //    //Use these mappings to get all weekly stats and their associated week number
                //    var weeklyStatTasks = playerMappings.Select(async m =>
                //    {
                //        //for each mapping, get the associated stat
                //        var statRes = await _supabase
                //            .From<PlayerStat>()
                //            .Where(s => s.Id == m.PlayerStatsId)
                //            .Get();
                //        var stat = statRes.Model;
                //        //from that stat, create a DTO
                //        var statDTO = new DTOs.PlayerStat
                //        {
                //            Completions = stat.Completions,
                //            PassingAttempts = stat.PassingAttempts,
                //            PassingYards = stat.PassingYards,
                //            PassingTds = stat.PassingTds,
                //            InterceptionsAgainst = stat.InterceptionsAgainst,
                //            SacksAgainst = stat.SacksAgainst,
                //            FumblesAgainst = stat.FumblesAgainst,
                //            PassingFirstDowns = stat.PassingFirstDowns,
                //            PassingEpa = stat.PassingEpa,
                //            Carries = stat.Carries,
                //            RushingYards = stat.RushingYards,
                //            RushingTds = stat.RushingTds,
                //            RushingFirstDowns = stat.RushingFirstDowns,
                //            RushingEpa = stat.RushingEpa,
                //            Receptions = stat.Receptions,
                //            Targets = stat.Targets,
                //            ReceivingYards = stat.ReceivingYards,
                //            ReceivingTds = stat.ReceivingTds,
                //            ReceivingFirstDowns = stat.ReceivingFirstDowns,
                //            ReceivingEpa = stat.ReceivingEpa,
                //            FgMadeList = stat.FgMadeList,
                //            FgMissedList = stat.FgMissedList,
                //            FgBlockedList = stat.FgBlockedList,
                //            PadAttempts = stat.PadAttempts,
                //            PatPercent = stat.PatPercent,
                //            FantasyPoints = stat.FantasyPoints,
                //            FantasyPointsPpr = stat.FantasyPointsPpr
                //        };
                //        //create a final DTO holding the stat DTO and its week number
                //        var statWithWeek = new DTOs.PlayerStatWithWeekNum
                //        {
                //            Week = m.Week,
                //            Stat = statDTO
                //        };
                //        return statWithWeek;
                //    }).ToList();
                //    //Since this query contains its own async operations, it can only
                //    // return tasks. The program needs to wait on those to finish to
                //    // get the final result before it moves on.
                //    var weeklyStatDTOs = (await Task.WhenAll(weeklyStatTasks)).ToList();

                //    return new DTOs.PlayerWithStats
                //    {
                //        Player = new DTOs.Player
                //        {
                //            Id = player.Id,
                //            Name = player.Name,
                //            HeadshotUrl = player.HeadshotUrl,
                //            Position = player.Position,
                //            Status = player.Status,
                //            StatusDescription = player.StatusDescription,
                //            TeamId = player.TeamId,
                //            SeasonStatsId = player.SeasonStatsId
                //        },
                //        SeasonStat = new DTOs.PlayerStat
                //        {
                //            Completions = seasonStats.Completions,
                //            PassingAttempts = seasonStats.PassingAttempts,
                //            PassingYards = seasonStats.PassingYards,
                //            PassingTds = seasonStats.PassingTds,
                //            InterceptionsAgainst = seasonStats.InterceptionsAgainst,
                //            SacksAgainst = seasonStats.SacksAgainst,
                //            FumblesAgainst = seasonStats.FumblesAgainst,
                //            PassingFirstDowns = seasonStats.PassingFirstDowns,
                //            PassingEpa = seasonStats.PassingEpa,
                //            Carries = seasonStats.Carries,
                //            RushingYards = seasonStats.RushingYards,
                //            RushingTds = seasonStats.RushingTds,
                //            RushingFirstDowns = seasonStats.RushingFirstDowns,
                //            RushingEpa = seasonStats.RushingEpa,
                //            Receptions = seasonStats.Receptions,
                //            Targets = seasonStats.Targets,
                //            ReceivingYards = seasonStats.ReceivingYards,
                //            ReceivingTds = seasonStats.ReceivingTds,
                //            ReceivingFirstDowns = seasonStats.ReceivingFirstDowns,
                //            ReceivingEpa = seasonStats.ReceivingEpa,
                //            FgMadeList = seasonStats.FgMadeList,
                //            FgMissedList = seasonStats.FgMissedList,
                //            FgBlockedList = seasonStats.FgBlockedList,
                //            PadAttempts = seasonStats.PadAttempts,
                //            PatPercent = seasonStats.PatPercent,
                //            FantasyPoints = seasonStats.FantasyPoints,
                //            FantasyPointsPpr = seasonStats.FantasyPointsPpr
                //        },
                //        WeeklyStats = weeklyStatDTOs
                //    };
                //}).ToList();
                //Make sure to wait for all tasks to be completed before returning.
                //List<DTOs.PlayerWithStats> playerDTOs = (await Task.WhenAll(playerTasks)).ToList();

                return Ok(playerWithStatsDTOs);
            }
        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}