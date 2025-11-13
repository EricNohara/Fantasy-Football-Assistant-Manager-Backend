using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Responses;
using static Supabase.Postgrest.Constants;

namespace Fantasy_Football_Assistant_Manager.Controllers.SupaBaseControllers

{
    //A helper class to hold what would have otherwise been a long stretch of redundant code
    public static class Helper
    {
        //A method to collect and return a list of DTOs holding players with their associated stats
        public static async Task<List<DTOs.PlayerWithStats>> GetPlayersWithStats(
            List<Player> players, //list of base player objects
            Client _supabase,    //the supabase client to search
            int? seasonStartYear = null    //the weekly stats season start year field to check for
            )
        {
            //First, retrieve all of the player-to-weeklyStats mappings.
            var playerIds = players.Select(p => p.Id).ToList(); //used to get all mappings associated with pulled players
            //The user may not want to filter by seasonStartYear. Only add that filter
            // if an argument was given.
            var mappingQuery = _supabase
                .From<WeeklyPlayerStat>()
                .Filter(x => x.PlayerId, Operator.In, playerIds);
            if (seasonStartYear != null)
            {
                mappingQuery = mappingQuery
                    .Filter(m => m.SeasonStartYear, Operator.Equals, seasonStartYear);
            }

            var mappingRes = await mappingQuery.Get();
            //var mappingRes = await _supabase
            //    .From<WeeklyPlayerStat>()
            //    .Where(m => m.SeasonStartYear == seasonStartYear)   //ensure this is from the right season
            //    .Filter(x => x.PlayerId, Operator.In, playerIds)
            //    .Get();
            var mappings = mappingRes.Models;

            //Second, use those mappings to retrieve all of the weekly stats.
            var mappingIds = mappings.Select(m => m.PlayerStatsId).ToList();    //used to get all stats associated with pulled players
            var statsRes = await _supabase
                .From<PlayerStat>()
                .Filter(x => x.Id, Operator.In, mappingIds)
                .Get();
            var stats = statsRes.Models;

            //Third, combine each player's weekly stats into a list and use all of the
            // collected info to form the playerWithStats DTOs.
            var playerTasks = players.Select(async player =>
            {
                //get this player's season stats
                var seasonStatsRes = await _supabase
                    .From<PlayerStat>()
                    .Where(s => s.Id == player.SeasonStatsId)
                    .Get();
                var seasonStats = seasonStatsRes.Model;
                //get this player's stats mappings, making sure to not select the season stats
                var playerMappings = mappings
                    .Where(m => m.PlayerId == player.Id && m.PlayerStatsId != player.SeasonStatsId)
                    .ToList();
                //Use these mappings to get all weekly stats and their associated week number
                var weeklyStatDTOs = playerMappings.Select(m =>
                {
                    //for each mapping, get the associated stat
                    var stat = stats
                        .Where(s => s.Id == m.PlayerStatsId)
                        .Single();
                    //from that stat, create a DTO
                    var statDTO = new DTOs.PlayerStat
                    {
                        Completions = stat.Completions,
                        PassingAttempts = stat.PassingAttempts,
                        PassingYards = stat.PassingYards,
                        PassingTds = stat.PassingTds,
                        InterceptionsAgainst = stat.InterceptionsAgainst,
                        SacksAgainst = stat.SacksAgainst,
                        FumblesAgainst = stat.FumblesAgainst,
                        PassingFirstDowns = stat.PassingFirstDowns,
                        PassingEpa = stat.PassingEpa,
                        Carries = stat.Carries,
                        RushingYards = stat.RushingYards,
                        RushingTds = stat.RushingTds,
                        RushingFirstDowns = stat.RushingFirstDowns,
                        RushingEpa = stat.RushingEpa,
                        Receptions = stat.Receptions,
                        Targets = stat.Targets,
                        ReceivingYards = stat.ReceivingYards,
                        ReceivingTds = stat.ReceivingTds,
                        ReceivingFirstDowns = stat.ReceivingFirstDowns,
                        ReceivingEpa = stat.ReceivingEpa,
                        FgMadeList = stat.FgMadeList,
                        FgMissedList = stat.FgMissedList,
                        FgBlockedList = stat.FgBlockedList,
                        PadAttempts = stat.PadAttempts,
                        PatPercent = stat.PatPercent,
                        FantasyPoints = stat.FantasyPoints,
                        FantasyPointsPpr = stat.FantasyPointsPpr
                    };
                    //create a final DTO holding the stat DTO and its week number
                    var statWithWeek = new DTOs.PlayerStatWithWeekNum
                    {
                        Week = m.Week,
                        Stat = statDTO
                    };
                    return statWithWeek;
                }).ToList();

                //return a DTO for this player with its stats
                return new DTOs.PlayerWithStats
                {
                    Player = new DTOs.Player
                    {
                        Id = player.Id,
                        Name = player.Name,
                        HeadshotUrl = player.HeadshotUrl,
                        Position = player.Position,
                        Status = player.Status,
                        StatusDescription = player.StatusDescription,
                        TeamId = player.TeamId,
                        SeasonStatsId = player.SeasonStatsId
                    },
                    SeasonStat = new DTOs.PlayerStat
                    {
                        Completions = seasonStats.Completions,
                        PassingAttempts = seasonStats.PassingAttempts,
                        PassingYards = seasonStats.PassingYards,
                        PassingTds = seasonStats.PassingTds,
                        InterceptionsAgainst = seasonStats.InterceptionsAgainst,
                        SacksAgainst = seasonStats.SacksAgainst,
                        FumblesAgainst = seasonStats.FumblesAgainst,
                        PassingFirstDowns = seasonStats.PassingFirstDowns,
                        PassingEpa = seasonStats.PassingEpa,
                        Carries = seasonStats.Carries,
                        RushingYards = seasonStats.RushingYards,
                        RushingTds = seasonStats.RushingTds,
                        RushingFirstDowns = seasonStats.RushingFirstDowns,
                        RushingEpa = seasonStats.RushingEpa,
                        Receptions = seasonStats.Receptions,
                        Targets = seasonStats.Targets,
                        ReceivingYards = seasonStats.ReceivingYards,
                        ReceivingTds = seasonStats.ReceivingTds,
                        ReceivingFirstDowns = seasonStats.ReceivingFirstDowns,
                        ReceivingEpa = seasonStats.ReceivingEpa,
                        FgMadeList = seasonStats.FgMadeList,
                        FgMissedList = seasonStats.FgMissedList,
                        FgBlockedList = seasonStats.FgBlockedList,
                        PadAttempts = seasonStats.PadAttempts,
                        PatPercent = seasonStats.PatPercent,
                        FantasyPoints = seasonStats.FantasyPoints,
                        FantasyPointsPpr = seasonStats.FantasyPointsPpr
                    },
                    WeeklyStats = weeklyStatDTOs
                };
            }).ToList();

            //wait for all player objects to finish being created and return the list
            return (await Task.WhenAll(playerTasks)).ToList();

            //    //first, get the player DTO
            //    var playerRes = await _supabase
            //        .From<Player>()
            //        .Where(p => p.Id == playerID)
            //        .Get();
            //    var player = playerRes.Model;
            //    var playerDTO = new DTOs.Player
            //    {
            //        Id = player.Id,
            //        Name = player.Name,
            //        HeadshotUrl = player.HeadshotUrl,
            //        Position = player.Position,
            //        Status = player.Status,
            //        StatusDescription = player.StatusDescription,
            //        TeamId = player.TeamId,
            //        SeasonStatsId = player.SeasonStatsId
            //    };

            //    //get the season stats DTO
            //    var seasonRes = await _supabase
            //        .From<PlayerStat>()
            //        .Where(s => s.Id == player.SeasonStatsId)
            //        .Get();
            //    var seasonStats = seasonRes.Model;
            //    var seasonDTO = new DTOs.PlayerStat
            //    {
            //        Completions = seasonStats.Completions,
            //        PassingAttempts = seasonStats.PassingAttempts,
            //        PassingYards = seasonStats.PassingYards,
            //        PassingTds = seasonStats.PassingTds,
            //        InterceptionsAgainst = seasonStats.InterceptionsAgainst,
            //        SacksAgainst = seasonStats.SacksAgainst,
            //        FumblesAgainst = seasonStats.FumblesAgainst,
            //        PassingFirstDowns = seasonStats.PassingFirstDowns,
            //        PassingEpa = seasonStats.PassingEpa,
            //        Carries = seasonStats.Carries,
            //        RushingYards = seasonStats.RushingYards,
            //        RushingTds = seasonStats.RushingTds,
            //        RushingFirstDowns = seasonStats.RushingFirstDowns,
            //        RushingEpa = seasonStats.RushingEpa,
            //        Receptions = seasonStats.Receptions,
            //        Targets = seasonStats.Targets,
            //        ReceivingYards = seasonStats.ReceivingYards,
            //        ReceivingTds = seasonStats.ReceivingTds,
            //        ReceivingFirstDowns = seasonStats.ReceivingFirstDowns,
            //        ReceivingEpa = seasonStats.ReceivingEpa,
            //        FgMadeList = seasonStats.FgMadeList,
            //        FgMissedList = seasonStats.FgMissedList,
            //        FgBlockedList = seasonStats.FgBlockedList,
            //        PadAttempts = seasonStats.PadAttempts,
            //        PatPercent = seasonStats.PatPercent,
            //        FantasyPoints = seasonStats.FantasyPoints,
            //        FantasyPointsPpr = seasonStats.FantasyPointsPpr
            //    };

            //    //get the list of weekly stats DTOs
            //    var mappingsRes = await _supabase
            //        .From<WeeklyPlayerStat>()
            //        .Where(m => m.PlayerId == playerID)
            //        .Get();
            //    var mappings = mappingsRes.Models;  //get the weekly stats mappings
            //    //use the mappings to get the weekly stats objects and their associated week
            //    var weeklyStatTasks = mappings.Select(async m =>
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
            //    //wait on all weekly stat DTOs to generate
            //    var weeklyStatDTOs = (await Task.WhenAll(weeklyStatTasks)).ToList();

            //    //compile all player data
            //    var playerWithStats = new DTOs.PlayerWithStats
            //    {
            //        Player = playerDTO,
            //        SeasonStat = seasonDTO,
            //        WeeklyStats = weeklyStatDTOs
            //    };

            //    return playerWithStats;
            //}
        }
    }
}
