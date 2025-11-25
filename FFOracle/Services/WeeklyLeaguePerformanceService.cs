using FFOracle.DTOs;
using FFOracle.Models.Supabase;
using FFOracle.Utils;
using Supabase;
using System.Text.Json;

namespace FFOracle.Services;

public class WeeklyLeaguePerformanceService
{
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
}
