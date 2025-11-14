using FFOracle.Models.Supabase;
using FFOracle.Utils;
using Square;
using Supabase;

namespace FFOracle.Services;

public class UpdateSupabaseService
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;

    public UpdateSupabaseService(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    // insert the last three weeks
    public async Task<(int startWeek, int endWeek)> UpdateStatsFromLastThreeWeeksAsync()
    {
        // get all weekly stats
        var weeklyStats = await _nflVerseService.GetAllOffensivePlayerWeeklyStatsAsync();
        if (weeklyStats == null || !weeklyStats.Any())
        {
            throw new Exception("No season stats found to insert.");
        }

        // Fetch the current_week from app_settings table
        var appStateResponse = await _supabase
            .From<AppState>()
            .Select("current_week")
            .Get();

        int currentWeek = appStateResponse.Models.FirstOrDefault()?.CurrentWeek ?? 1;

        // Compute the week range: last 3 completed weeks
        int startWeek = Math.Max(currentWeek - 3, 1);
        int endWeek = Math.Max(currentWeek - 1, 1);

        // Fetch all existing players from the players table
        var existingPlayersResponse = await _supabase
            .From<Player>()
            .Select("id")
            .Get();

        var existingPlayerIds = existingPlayersResponse.Models.Select(p => p.Id).ToHashSet();

        // Filter stats to only include existing players and new weeks
        var newStats = weeklyStats
            .Where(s => s.Week >= startWeek && s.Week <= endWeek && existingPlayerIds.Contains(s.PlayerId))
            .ToList();

        // add new stats if they exist
        if (newStats.Any())
        {
            // convert the newStats to PlayerStats model while generating list of WeeklyPlayerStat models
            List<WeeklyPlayerStat> weeklyPlayerStats = new List<WeeklyPlayerStat>();

            var playerStats = newStats
                .Select(p =>
                {
                    var id = Guid.NewGuid();

                    // create and insert new WeeklyPlayerStat model
                    var weeklyStat = new WeeklyPlayerStat
                    {
                        Id = Guid.NewGuid(),
                        PlayerStatsId = id,
                        PlayerId = p.PlayerId,
                        Week = p.Week,
                        SeasonStartYear = p.SeasonStartYear,
                    };

                    weeklyPlayerStats.Add(weeklyStat);

                    return new PlayerStat
                    {
                        Id = id,
                        Completions = ControllerHelpers.NullIfZero(p.Completions),
                        PassingAttempts = ControllerHelpers.NullIfZero(p.PassingAttempts),
                        PassingYards = ControllerHelpers.NullIfZero(p.PassingYards),
                        PassingTds = ControllerHelpers.NullIfZero(p.PassingTds),
                        InterceptionsAgainst = ControllerHelpers.NullIfZero(p.InterceptionsAgainst),
                        SacksAgainst = ControllerHelpers.NullIfZero(p.SacksAgainst),
                        FumblesAgainst = ControllerHelpers.NullIfZero(p.FumblesAgainst),
                        PassingFirstDowns = ControllerHelpers.NullIfZero(p.PassingFirstDowns),
                        PassingEpa = ControllerHelpers.NullIfZero(p.PassingEpa),
                        Carries = ControllerHelpers.NullIfZero(p.Carries),
                        RushingYards = ControllerHelpers.NullIfZero(p.RushingYards),
                        RushingTds = ControllerHelpers.NullIfZero(p.RushingTds),
                        RushingFirstDowns = ControllerHelpers.NullIfZero(p.RushingFirstDowns),
                        RushingEpa = ControllerHelpers.NullIfZero(p.RushingEpa),
                        Receptions = ControllerHelpers.NullIfZero(p.Receptions),
                        Targets = ControllerHelpers.NullIfZero(p.Targets),
                        ReceivingYards = ControllerHelpers.NullIfZero(p.ReceivingYards),
                        ReceivingTds = ControllerHelpers.NullIfZero(p.ReceivingTds),
                        ReceivingFirstDowns = ControllerHelpers.NullIfZero(p.ReceivingFirstDowns),
                        ReceivingEpa = ControllerHelpers.NullIfZero(p.ReceivingEpa),
                        FgMadeList = p.FgMadeList,
                        FgMissedList = p.FgMissedList,
                        FgBlockedList = p.FgBlockedList,
                        PadAttempts = ControllerHelpers.NullIfZero(p.PadAttempts),
                        PatPercent = ControllerHelpers.NullIfZero(p.PatPercent),
                        FantasyPoints = p.FantasyPoints,
                        FantasyPointsPpr = p.FantasyPointsPpr
                    };
                })
                .ToList();

            // insert PlayerStat models into player_stats table
            var insertedStatsResponse = await _supabase
                .From<PlayerStat>()
                .Insert(playerStats);

            // insert new relational record into weekly_player_stats table
            var insertedWeeklyResponse = await _supabase
                .From<WeeklyPlayerStat>()
                .Insert(weeklyPlayerStats);
        }

        // delete all old weekly stats
        await _supabase.Rpc("delete_old_week_stats", new { min_week = startWeek });

        return (startWeek, endWeek);
    }

    // update all player's season stats
    public async Task UpdateAllPlayerSeasonStatsAsync()
    {
        // fetch offensive players using service
        var seasonStats = await _nflVerseService.GetAllOffensivePlayerSeasonStatsAsync();
        if (seasonStats == null || !seasonStats.Any())
        {
            throw new Exception("No season stats found to insert.");
        }

        // get pairs of player_id and their season_stats_id
        var playerIds = seasonStats.Select(s => s.PlayerId).ToList();

        var playersResponse = await _supabase
            .From<Player>()
            .Select(p => new object[] { p.Id, p.SeasonStatsId })
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, playerIds)
            .Get();

        var playerStatMap = playersResponse.Models
            .Where(p => p.SeasonStatsId != null)
            .ToDictionary(p => p.Id, p => p.SeasonStatsId.Value);

        // Build list of updates
        var updates = seasonStats
            .Where(s => playerStatMap.ContainsKey(s.PlayerId))
            .Select(s => new
            {
                season_stats_id = playerStatMap[s.PlayerId],
                completions = ControllerHelpers.NullIfZero(s.Completions),
                passing_attempts = ControllerHelpers.NullIfZero(s.PassingAttempts),
                passing_yards = ControllerHelpers.NullIfZero(s.PassingYards),
                passing_tds = ControllerHelpers.NullIfZero(s.PassingTds),
                interceptions_against = ControllerHelpers.NullIfZero(s.InterceptionsAgainst),
                sacks_against = ControllerHelpers.NullIfZero(s.SacksAgainst),
                fumbles_against = ControllerHelpers.NullIfZero(s.FumblesAgainst),
                passing_first_downs = ControllerHelpers.NullIfZero(s.PassingFirstDowns),
                passing_epa = ControllerHelpers.NullIfZero(s.PassingEpa),
                carries = ControllerHelpers.NullIfZero(s.Carries),
                rushing_yards = ControllerHelpers.NullIfZero(s.RushingYards),
                rushing_tds = ControllerHelpers.NullIfZero(s.RushingTds),
                rushing_first_downs = ControllerHelpers.NullIfZero(s.RushingFirstDowns),
                rushing_epa = ControllerHelpers.NullIfZero(s.RushingEpa),
                receptions = ControllerHelpers.NullIfZero(s.Receptions),
                targets = ControllerHelpers.NullIfZero(s.Targets),
                receiving_yards = ControllerHelpers.NullIfZero(s.ReceivingYards),
                receiving_tds = ControllerHelpers.NullIfZero(s.ReceivingTds),
                receiving_first_downs = ControllerHelpers.NullIfZero(s.ReceivingFirstDowns),
                receiving_epa = ControllerHelpers.NullIfZero(s.ReceivingEpa),
                fg_made_list = s.FgMadeList,
                fg_missed_list = s.FgMissedList,
                fg_blocked_list = s.FgBlockedList,
                pat_attempts = ControllerHelpers.NullIfZero(s.PadAttempts),
                pat_percent = ControllerHelpers.NullIfZero(s.PatPercent),
                fantasy_points = s.FantasyPoints,
                fantasy_points_ppr = s.FantasyPointsPpr
            }).ToList();

        // Call RPC to update all at once
        await _supabase.Rpc("batch_update_player_season_stats", new { updates });

        return;
    }

    // update all player's non stat data
    public async Task UpdateAllPlayerNonStatDataAsync()
    {
        // get all player ids from supabase
        var response = await _supabase
            .From<Player>()
            .Get();
        if (response.Models == null || !response.Models.Any())
        {
            throw new Exception("No players found in database.");
        }

        var existingPlayers = response.Models;
        var playerLookup = existingPlayers.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

        // get the current season and week from supabase
        var (currentSeason, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);

        // get all player information from nflverse service
        var playerInformation = await _nflVerseService.GetAllPlayerInformationAsync(currentSeason);

        if (playerInformation == null || !playerInformation.Any())
        {
            throw new Exception("No player information found.");
        }

        // merge info only for players that exist in Supabase
        var filteredPlayerWithInfo = playerInformation
            .Where(info => !string.IsNullOrWhiteSpace(info.Id) && playerLookup.ContainsKey(info.Id))
            .Select(info =>
            {
                var existingPlayer = playerLookup[info.Id];
                return new Player
                {
                    Id = existingPlayer.Id,
                    Name = existingPlayer.Name,
                    HeadshotUrl = existingPlayer.HeadshotUrl,
                    Position = existingPlayer.Position,
                    Status = info.Status,
                    StatusDescription = info.ShortDescription,
                    TeamId = info.LatestTeam,
                    SeasonStatsId = existingPlayer.SeasonStatsId
                };
            })
            .ToList();

        if (!filteredPlayerWithInfo.Any())
        {
            throw new Exception("No matching players to update.");
        }

        // update Supabase players table
        var updateResponse = await _supabase
            .From<Player>()
            .OnConflict(x => x.Id)
            .Upsert(filteredPlayerWithInfo);

        return;
    }

    // update team offensive stats
    public async Task UpdateAllTeamSeasonStatsAsync()
    {
        // fetch team stats using service
        var stats = await _nflVerseService.GetAllTeamSeasonStatsAsync();
        if (stats == null || !stats.Any())
        {
            throw new Exception("No team stats found to insert.");
        }

        // break up the stats into offensive and defensive stats, keeping track of the new ids
        List<TeamOffensiveStat> offensiveStats = new List<TeamOffensiveStat>();
        List<TeamDefensiveStat> defensiveStats = new List<TeamDefensiveStat>();
        Dictionary<string, Guid> teamOffensiveStatIdMap = new Dictionary<string, Guid>();
        Dictionary<string, Guid> teamDefensiveStatIdMap = new Dictionary<string, Guid>();


        foreach (var stat in stats)
        {
            if (stat == null) continue;

            // add the ids to the corresponding maps
            Guid offensiveId = Guid.NewGuid();
            Guid defensiveId = Guid.NewGuid();
            teamOffensiveStatIdMap[stat.Team] = offensiveId;
            teamDefensiveStatIdMap[stat.Team] = defensiveId;

            // parse out the offensive stats and add to list
            var offensiveStat = new TeamOffensiveStat
            {
                Id = offensiveId,
                Completions = ControllerHelpers.NullIfZero(stat.Completions),
                Attempts = ControllerHelpers.NullIfZero(stat.Attempts),
                PassingYards = ControllerHelpers.NullIfZero(stat.PassingYards),
                PassingTds = ControllerHelpers.NullIfZero(stat.PassingTds),
                PassingInterceptions = ControllerHelpers.NullIfZero(stat.PassingInterceptions),
                SacksAgainst = ControllerHelpers.NullIfZero(stat.SacksAgainst),
                FumblesAgainst = ControllerHelpers.NullIfZero(stat.FumblesAgainst),
                Carries = ControllerHelpers.NullIfZero(stat.Carries),
                RushingYards = ControllerHelpers.NullIfZero(stat.RushingYards),
                RushingTds = ControllerHelpers.NullIfZero(stat.RushingTds),
                Receptions = ControllerHelpers.NullIfZero(stat.Receptions),
                Targets = ControllerHelpers.NullIfZero(stat.Targets),
                ReceivingYards = ControllerHelpers.NullIfZero(stat.ReceivingYards),
                ReceivingTds = ControllerHelpers.NullIfZero(stat.ReceivingTds)
            };

            offensiveStats.Add(offensiveStat);

            // parse out the defensive stats and add to list
            var defensiveStat = new TeamDefensiveStat
            {
                Id = defensiveId,
                TacklesForLoss = ControllerHelpers.NullIfZero(stat.TacklesForLoss),
                TacklesForLossYards = ControllerHelpers.NullIfZero(stat.TacklesForLossYards),
                FumblesFor = ControllerHelpers.NullIfZero(stat.FumblesFor),
                SacksFor = ControllerHelpers.NullIfZero(stat.SacksFor),
                SackYardsFor = ControllerHelpers.NullIfZero(stat.SackYardsFor),
                InterceptionsFor = ControllerHelpers.NullIfZero(stat.InterceptionsFor),
                InterceptionYardsFor = ControllerHelpers.NullIfZero(stat.InterceptionYardsFor),
                DefTds = ControllerHelpers.NullIfZero(stat.DefTds),
                Safeties = ControllerHelpers.NullIfZero(stat.Safeties),
                PassDefended = ControllerHelpers.NullIfZero(stat.PassDefended)
            };

            defensiveStats.Add(defensiveStat);
        }

        // get existing teams from supabase
        var teamsResponse = await _supabase
            .From<Team>()
            .Get();

        if (teamsResponse.Models == null || !teamsResponse.Models.Any())
        {
            throw new Exception("No teams found in the database");
        }

        var existingTeams = teamsResponse.Models;

        // create updated teams list
        var updatedTeams = existingTeams
            .Where(t => teamOffensiveStatIdMap.ContainsKey(t.Id))
            .Select(t =>
                new Team
                {
                    Id = t.Id,
                    Name = t.Name,
                    Conference = t.Conference,
                    Division = t.Division,
                    LogoUrl = t.LogoUrl,
                    OffensiveStatsId = teamOffensiveStatIdMap[t.Id],
                    DefensiveStatsId = teamDefensiveStatIdMap[t.Id]
                }
            )
            .ToList();

        // insert the offensive stats
        await _supabase
            .From<TeamOffensiveStat>()
            .Insert(offensiveStats);

        // insert the defensive stats
        await _supabase
            .From<TeamDefensiveStat>()
            .Insert(defensiveStats);

        // update the team records to reference the stats
        await _supabase
            .From<Team>()
            .OnConflict(x => x.Id)
            .Upsert(updatedTeams);

        return;
    }

    // update games this week
    public async Task UpdateGamesThisWeekAsync()
    {
        // get current season and week from supabase
        var (currentSeason, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);

        // get game data from nfl verse service
        var games = await _nflVerseService.GetAllGamesThisWeekAsync(currentSeason, currentWeek);

        if (games == null || !games.Any())
        {
            throw new Exception("No games found for current week.");
        }

        // map GameThisWeekCsv to GameThisWeek supabase model
        var gamesToInsert = games.Select(g => new GameThisWeek
        {
            Id = Guid.NewGuid(),
            HomeTeam = g.HomeTeam,
            AwayTeam = g.AwayTeam,
            Weekday = g.Weekday,
            GameDateTime = g.GameDateTime,
            StadiumName = g.StadiumName,
            StadiumStyle = g.StadiumStyle,
            IsDivisionalGame = g.IsDivisionalGame,
            HomeRestDays = g.HomeRestDays,
            AwayRestDays = g.AwayRestDays,
            HomeMoneyline = g.HomeMoneyline,
            AwayMoneyline = g.AwayMoneyline,
            HomeSpreadOdds = g.HomeSpreadOdds,
            AwaySpreadOdds = g.AwaySpreadOdds,
            SpreadLine = g.SpreadLine,
            TotalLine = g.TotalLine,
            UnderOdds = g.UnderOdds,
            OverOdds = g.OverOdds
        }).ToList();

        // clear the games this week table and repopulate it
        await _supabase
            .From<GameThisWeek>()
            .Where(g => g.Id != null)
            .Delete();

        await _supabase
            .From<GameThisWeek>()
            .Insert(gamesToInsert);

        return;
    }

    // Add new players from the CSV/service that do NOT exist in the database - do this for new players in their DB appearing
    public async Task AddNewPlayersAsync()
    {
        // fetch all players from nfl verse service
        var allPlayersFromService = await _nflVerseService.GetAllOffensivePlayersAsync();
        if (allPlayersFromService == null || !allPlayersFromService.Any())
        {
            throw new Exception("No players found to insert.");
        }

        // fetch all existing players from Supabase
        var existingPlayersResponse = await _supabase
            .From<Player>()
            .Select("id")
            .Get();

        var existingPlayerIds = existingPlayersResponse.Models
            .Select(p => p.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // filter out players that already exist
        var newPlayers = allPlayersFromService
            .Where(p => !existingPlayerIds.Contains(p.Id))
            .ToList();

        // nothing new to add
        if (!newPlayers.Any())
        {
            return;
        }

        // optionally, fetch detailed player info for these new players
        var currentSeason = (await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase)).Season;
        var playerInfo = await _nflVerseService.GetAllPlayerInformationAsync(currentSeason);
        var playerInfoLookup = playerInfo.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

        // create season stats for each new player
        var playerStatsToInsert = newPlayers
            .Select(p =>
            {
                var statId = Guid.NewGuid();
                return new PlayerStat
                {
                    Id = statId,
                    // initialize all numeric fields to null
                    Completions = null,
                    PassingAttempts = null,
                    PassingYards = null,
                    PassingTds = null,
                    InterceptionsAgainst = null,
                    SacksAgainst = null,
                    FumblesAgainst = null,
                    PassingFirstDowns = null,
                    PassingEpa = null,
                    Carries = null,
                    RushingYards = null,
                    RushingTds = null,
                    RushingFirstDowns = null,
                    RushingEpa = null,
                    Receptions = null,
                    Targets = null,
                    ReceivingYards = null,
                    ReceivingTds = null,
                    ReceivingFirstDowns = null,
                    ReceivingEpa = null,
                    FgMadeList = null,
                    FgMissedList = null,
                    FgBlockedList = null,
                    PadAttempts = null,
                    PatPercent = null,
                    FantasyPoints = null,
                    FantasyPointsPpr = null
                };
            })
            .ToList();

        // insert season stats into Supabase
        await _supabase
            .From<PlayerStat>()
            .Insert(playerStatsToInsert);

        // map player ID to their season stat ID
        var statIdMap = playerStatsToInsert
            .Select((stat, index) => new { PlayerId = newPlayers[index].Id, StatId = stat.Id })
            .ToDictionary(x => x.PlayerId, x => x.StatId);

        // create new players with SeasonStatsId
        var playersToInsert = newPlayers
            .Select(p =>
            {
                playerInfoLookup.TryGetValue(p.Id, out var info);

                return new Player
                {
                    Id = p.Id,
                    Name = p.Name,
                    HeadshotUrl = p.HeadshotUrl,
                    Position = p.Position,
                    Status = info?.Status,
                    StatusDescription = info?.ShortDescription,
                    TeamId = info?.LatestTeam,
                    SeasonStatsId = statIdMap[p.Id]  // link to the newly created stat
                };
            })
            .ToList();

        // insert new players into Supabase
        await _supabase
            .From<Player>()
            .Insert(playersToInsert);
    }

}
