using FFOracle.DTOs;
using FFOracle.Models.Supabase;
using FFOracle.Utils;
using Supabase;
using System.Text.Json;

namespace FFOracle.Services;

public class WeeklyLeaguePerformanceService
{
    private readonly HashSet<string> flexEligible = new HashSet<string> { "RB", "WR", "TE" };
    private readonly Client _supabase;

    public WeeklyLeaguePerformanceService(Client supabase)
    {
        _supabase = supabase;
    }

    public async Task UpsertWeeklyLeaguePerformanceAsync(Guid leagueId, bool isPpr)
    {
        // get the current week
        var (currentSeason, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);
        short targetWeek = (short)(currentWeek - 1);

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

        // Get actual FPTS for picked players
        var pickedResult = await _supabase.Rpc("get_weekly_fantasy_points", new
        {
            _player_ids = picked,
            _week = targetWeek,
            _is_ppr = isPpr
        });

        var pickedRows = JsonSerializer.Deserialize<List<WeeklyFantasyRow>>(
            pickedResult.Content.ToString()
        ) ?? new List<WeeklyFantasyRow>();

        double actualFpts = pickedRows.Sum(r => r.FantasyPoints);

        // Get max FPTS for *all* players
        var maxResult = await _supabase.Rpc("get_weekly_max_fantasy_points", new
        {
            _player_ids = allPlayers,
            _week = targetWeek,
            _is_ppr = isPpr
        });

        var maxRows = JsonSerializer.Deserialize<List<WeeklyFantasyRow>>(
            maxResult.Content.ToString()
        ) ?? new List<WeeklyFantasyRow>();

        double maxFpts = maxRows.Sum(r => r.FantasyPoints);


        // Accuracy calculation
        double accuracy = maxFpts > 0 ? (actualFpts / maxFpts) * 100.0 : 0.0;


        // Build upsert object
        var weeklyPerformance = new WeeklyLeaguePerformance
        {
            LeagueId = leagueId,
            Week = targetWeek,
            ActualFpts = actualFpts,
            MaxFpts = maxFpts,
            Accuracy = accuracy
        };


        // Upsert to Supabase
        await _supabase
            .From<WeeklyLeaguePerformance>()
            .Upsert(weeklyPerformance);
    }

    public async Task UpsertWeeklyLeaguePlayerPerformanceAsync(Guid leagueId, bool isPpr)
    {
        // 1. Get the current week
        var (_, currentWeek) = await ControllerHelpers.GetCurrentSeasonAndWeekAsync(_supabase);
        short targetWeek = (short)(currentWeek - 1);

        // 2. Load league + roster settings
        var league = await _supabase
            .From<Models.Supabase.UserLeague>()
            .Where(x => x.Id == leagueId)
            .Single();

        var roster = await _supabase
            .From<Models.Supabase.RosterSetting>()
            .Where(x => x.Id == league.RosterSettingsID)
            .Single();

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
            .Where(p => allPlayers.Contains(p.Id))
            .Get();

        var positionMap = playerData.Models
            .ToDictionary(p => p.Id, p => p.Position);

        // 5. Load fantasy points
        var fantasyResult = await _supabase.Rpc("get_weekly_max_fantasy_points", new
        {
            _player_ids = allPlayers,
            _week = targetWeek,
            _is_ppr = isPpr
        });

        var fantasyRows = JsonSerializer.Deserialize<List<WeeklyFantasyRow>>(
            fantasyResult.Content.ToString()
        ) ?? new List<WeeklyFantasyRow>();

        // 6. Group by position and rank
        var groupsByPosition = fantasyRows
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

        var flexEligible = new HashSet<string> { "RB", "WR", "TE" };

        // 7. Determine positional correctness
        var positionalCorrect = new HashSet<string>();

        foreach (var pos in groupsByPosition.Keys)
        {
            int allowed = GetAllowedSlots(pos);
            var rankedPos = groupsByPosition[pos];

            for (int i = 0; i < rankedPos.Count && i < allowed; i++)
                positionalCorrect.Add(rankedPos[i].PlayerId);
        }

        // 8. Build Flex Pool (leftovers only)
        var flexPool = new List<WeeklyFantasyRow>();

        foreach (var pos in flexEligible)
        {
            if (!groupsByPosition.ContainsKey(pos))
                continue;

            int allowed = GetAllowedSlots(pos);
            var ranked = groupsByPosition[pos];

            // leftover players after positional starters
            var leftover = ranked.Skip(allowed).ToList();
            flexPool.AddRange(leftover);
        }

        flexPool = flexPool.OrderByDescending(r => r.FantasyPoints).ToList();

        // 9. Flex correctness
        int usedFlexEligibleSlots = positionalCorrect
            .Count(pid => flexEligible.Contains(positionMap[pid]));

        int flexAvailable = roster.FlexCount - usedFlexEligibleSlots;
        if (flexAvailable < 0) flexAvailable = 0;

        var flexCandidates = flexPool
            .Where(r => !positionalCorrect.Contains(r.PlayerId))
            .ToList();

        var flexCorrect = new HashSet<string>();

        for (int i = 0; i < Math.Min(flexAvailable, flexCandidates.Count); i++)
            flexCorrect.Add(flexCandidates[i].PlayerId);

        bool IsCorrect(string pid) =>
            positionalCorrect.Contains(pid) ||
            flexCorrect.Contains(pid);

        // 10. Build maxFpts mapping
        var maxMap = new Dictionary<string, double>();

        // -------- Per-position maxFpts --------
        foreach (var pos in groupsByPosition.Keys)
        {
            var ranked = groupsByPosition[pos];
            int allowed = GetAllowedSlots(pos);

            if (ranked.Count == 0) continue;

            double bestStarterScore =
                allowed > 0 && ranked.Count >= allowed
                    ? ranked[allowed - 1].FantasyPoints
                    : ranked[0].FantasyPoints;

            for (int i = 0; i < ranked.Count; i++)
            {
                var row = ranked[i];

                if (i < allowed)
                    maxMap[row.PlayerId] = row.FantasyPoints;  // correct positional starter
                else
                    maxMap[row.PlayerId] = bestStarterScore;   // wrong positional choice
            }
        }

        // -------- Flex maxFpts --------
        if (flexAvailable > 0 && flexCandidates.Count > 0)
        {
            double bestFlexScore = flexCandidates[0].FantasyPoints;

            foreach (var fc in flexCandidates)
            {
                if (flexCorrect.Contains(fc.PlayerId))
                    maxMap[fc.PlayerId] = fc.FantasyPoints;  // correct flex starter
                else
                    maxMap[fc.PlayerId] = bestFlexScore;     // incorrect flex choice
            }
        }

        // 11. Build DB rows
        var rows = new List<WeeklyLeaguePlayerPerformance>();

        foreach (var row in fantasyRows)
        {
            rows.Add(new WeeklyLeaguePlayerPerformance
            {
                LeagueId = leagueId,
                Week = targetWeek,
                PlayerId = row.PlayerId,
                ActualFpts = row.FantasyPoints,
                MaxFpts = maxMap[row.PlayerId],
                WasCorrect = IsCorrect(row.PlayerId)
            });
        }

        // 12. Upsert to DB
        await _supabase
            .From<WeeklyLeaguePlayerPerformance>()
            .Upsert(rows);
    }
}
