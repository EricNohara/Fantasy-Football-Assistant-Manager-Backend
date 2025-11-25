using FFOracle.DTOs;
using FFOracle.Models.Supabase;
using FFOracle.Utils;
using Supabase;
using System.Text.Json;

namespace FFOracle.Services;

public class WeeklyLeaguePerformanceService
{
    private readonly HashSet<string> FLEX_ELIGIBLE = new HashSet<string> { "RB", "WR", "TE" };
    private readonly Client _supabase;

    public WeeklyLeaguePerformanceService(Client supabase)
    {
        _supabase = supabase;
    }

    public async Task UpsertWeeklyLeaguePerformanceAsync(Guid leagueId, bool isPpr = true)
    {
        // 1. Get the current week
        var (_, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);
        short targetWeek = (short)(currentWeek - 1);

        // 2. Get league offensive members
        var offensiveMembers = await _supabase
            .From<LeagueOffensiveMember>()
            .Where(m => m.LeagueId == leagueId)
            .Get();

        var picked = offensiveMembers.Models
            .Where(m => m.Picked)
            .Select(m => m.PlayerId)
            .ToArray();

        var allPlayers = offensiveMembers.Models
            .Select(m => m.PlayerId)
            .ToArray();

        // Handle empty rosters safely
        if (allPlayers.Length == 0)
        {
            var emptyPerformance = new WeeklyLeaguePerformance
            {
                LeagueId = leagueId,
                Week = targetWeek,
                ActualFpts = 0,
                MaxFpts = 0,
                Accuracy = 0
            };

            await _supabase.From<WeeklyLeaguePerformance>().Upsert(emptyPerformance);
            return;
        }

        // 3. Load league + roster settings
        var league = await _supabase
            .From<Models.Supabase.UserLeague>()
            .Where(x => x.Id == leagueId)
            .Single();

        if (league == null) return;

        var roster = await _supabase
            .From<Models.Supabase.RosterSetting>()
            .Where(x => x.Id == league.RosterSettingsID)
            .Single();

        if (roster == null) return;

        // 4. Load player positions
        var playerData = await _supabase
            .From<Models.Supabase.Player>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, allPlayers)
            .Get();

        var positionMap = playerData.Models
            .ToDictionary(p => p.Id, p => p.Position);

        // 5. Get actual FPTS for picked players
        double actualFpts = 0;
        if (picked.Length > 0)
        {
            var pickedResult = await _supabase.Rpc("get_weekly_fantasy_points", new
            {
                _player_ids = picked,
                _week = targetWeek,
                _is_ppr = isPpr
            });

            var pickedRows = JsonSerializer.Deserialize<List<WeeklyFantasyRow>>(
                pickedResult.Content.ToString()
            ) ?? new List<WeeklyFantasyRow>();

            actualFpts = pickedRows.Sum(r => r.FantasyPoints);
        }

        // 6. Get FPTS for *all* players (used for max lineup)
        var maxResult = await _supabase.Rpc("get_weekly_fantasy_points", new
        {
            _player_ids = allPlayers,
            _week = targetWeek,
            _is_ppr = isPpr
        });

        var allRows = JsonSerializer.Deserialize<List<WeeklyFantasyRow>>(
            maxResult.Content.ToString()
        ) ?? new List<WeeklyFantasyRow>();

        // 7. Group by position and sort descending by fantasy points
        var groupsByPosition = allRows
            .GroupBy(r => positionMap[r.PlayerId])
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.FantasyPoints).ToList()
            );

        int GetAllowedSlots(string pos) =>
            pos switch
            {
                "QB" => roster.QbCount,
                "RB" => roster.RbCount,
                "WR" => roster.WrCount,
                "TE" => roster.TeCount,
                "K" => roster.KCount,
                _ => 0
            };

        // 8. Build optimal lineup: first positional slots
        double maxFpts = 0;
        var selected = new HashSet<string>(); // players already used in optimal lineup

        foreach (var pos in groupsByPosition.Keys)
        {
            int allowed = GetAllowedSlots(pos);
            if (allowed <= 0) continue;

            var ranked = groupsByPosition[pos];

            for (int i = 0; i < Math.Min(allowed, ranked.Count); i++)
            {
                var row = ranked[i];
                if (selected.Add(row.PlayerId))
                {
                    maxFpts += row.FantasyPoints;
                }
            }
        }

        // 9. FLEX: best remaining RB/WR/TE not already selected
        var flexCandidates = allRows
            .Where(r => FLEX_ELIGIBLE.Contains(positionMap[r.PlayerId]) && !selected.Contains(r.PlayerId))
            .OrderByDescending(r => r.FantasyPoints)
            .ToList();

        int flexAllowed = roster.FlexCount;
        for (int i = 0; i < Math.Min(flexAllowed, flexCandidates.Count); i++)
        {
            var row = flexCandidates[i];
            if (selected.Add(row.PlayerId))
            {
                maxFpts += row.FantasyPoints;
            }
        }

        // 10. Accuracy calculation
        double accuracy = maxFpts > 0 ? (actualFpts / maxFpts) * 100.0 : 0.0;

        // 11. Build upsert object
        var weeklyPerformance = new WeeklyLeaguePerformance
        {
            LeagueId = leagueId,
            Week = targetWeek,
            ActualFpts = actualFpts,
            MaxFpts = maxFpts,
            Accuracy = accuracy
        };

        // 12. Upsert to Supabase
        await _supabase
            .From<WeeklyLeaguePerformance>()
            .Upsert(weeklyPerformance);
    }


    public async Task UpsertWeeklyLeaguePlayerPerformanceAsync(Guid leagueId, bool isPpr = true)
    {
        // 1. Get the current week
        var (_, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);
        short targetWeek = (short)(currentWeek - 1);

        // 2. Load league + roster settings
        var league = await _supabase
            .From<Models.Supabase.UserLeague>()
            .Where(x => x.Id == leagueId)
            .Single();

        if (league == null) return;

        var roster = await _supabase
            .From<Models.Supabase.RosterSetting>()
            .Where(x => x.Id == league.RosterSettingsID)
            .Single();

        if (roster == null) return;

        // 3. Load offensive members
        var offensiveMembers = await _supabase
            .From<LeagueOffensiveMember>()
            .Where(m => m.LeagueId == leagueId)
            .Get();

        var allPlayers = offensiveMembers.Models
            .Select(m => m.PlayerId)
            .ToArray();

        if (allPlayers.Length == 0)
            return;

        // 4. Load player positions
        var playerData = await _supabase
            .From<Models.Supabase.Player>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, allPlayers)
            .Get();

        var positionMap = playerData.Models.ToDictionary(p => p.Id, p => p.Position);

        // 5. Load fantasy points for all players
        var fantasyResult = await _supabase.Rpc("get_weekly_fantasy_points", new
        {
            _player_ids = allPlayers,
            _week = targetWeek,
            _is_ppr = isPpr
        });

        var fantasyRows = JsonSerializer.Deserialize<List<WeeklyFantasyRow>>(
            fantasyResult.Content.ToString()
        ) ?? new List<WeeklyFantasyRow>();

        // 6. Group by position and sort descending
        var groupsByPosition = fantasyRows
            .GroupBy(r => positionMap[r.PlayerId])
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.FantasyPoints).ToList()
            );

        // 7. Compute overall rank
        var overallRanked = fantasyRows
            .OrderByDescending(r => r.FantasyPoints)
            .ToList();

        var overallRankMap = new Dictionary<string, int>();
        for (int i = 0; i < overallRanked.Count; i++)
            overallRankMap[overallRanked[i].PlayerId] = i + 1;

        // 8. Compute positional rank
        var positionRankMap = new Dictionary<string, int>();

        foreach (var pos in groupsByPosition.Keys)
        {
            var ranked = groupsByPosition[pos];
            for (int i = 0; i < ranked.Count; i++)
                positionRankMap[ranked[i].PlayerId] = i + 1;
        }

        // 9. Build DB rows
        var rows = new List<WeeklyLeaguePlayerPerformance>();

        foreach (var row in fantasyRows)
        {
            rows.Add(new WeeklyLeaguePlayerPerformance
            {
                LeagueId = leagueId,
                Week = targetWeek,
                PlayerId = row.PlayerId,
                ActualFpts = row.FantasyPoints,
                Picked = offensiveMembers.Models.First(m => m.PlayerId == row.PlayerId).Picked,
                PositionRank = positionRankMap[row.PlayerId],
                OverallRank = overallRankMap[row.PlayerId]
            });
        }

        // 10. Upsert to DB
        await _supabase
            .From<WeeklyLeaguePlayerPerformance>()
            .Upsert(rows);
    }


    public async Task UpsertAllLeaguesWeeklyPerformanceAsync()
    {
        // get ALL leagues in DB
        var leagues = await _supabase
            .From<Models.Supabase.UserLeague>()
            .Get();

        foreach (var league in leagues.Models)
        {
            await UpsertWeeklyLeaguePerformanceAsync(league.Id);
            await UpsertWeeklyLeaguePlayerPerformanceAsync(league.Id);
        }
    }
}
