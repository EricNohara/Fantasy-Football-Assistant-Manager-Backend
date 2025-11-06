using FFOracle.Models.Supabase;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace FFOracle.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupaBaseController : ControllerBase
{
    private readonly Client _supabase;

    // Allowed tables that this controller may expose. Update as needed.
    private static readonly HashSet<string> AllowedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "users",
        "players",
        "player_stats",
        "teams",
        "team_members",
        "weekly_player_stats",
        "scoring_settings",
        "roster_settings",
        "team_offensive_stats",
        "team_defensive_stats"
    };

    public SupaBaseController(Client supabase)
    {
        _supabase = supabase;
    }

    /// Generic read-only endpoint to fetch all rows from a small, allow-listed set of Supabase tables.
    /// This maps table names to typed model queries to preserve schema and serialization behavior.
    [HttpGet("{table}")]
    public async Task<IActionResult> GetTableData(string table)
    {
        if (string.IsNullOrWhiteSpace(table) || !AllowedTables.Contains(table))
            return BadRequest(new { error = "Invalid or disallowed table name" });

        try
        {
            // Map table name to typed Supabase model queries, then convert to DTOs for safe JSON serialization.
            switch (table.ToLowerInvariant())
            {
                case "users":
                {
                    var res = await _supabase.From<User>().Get(); // fetch data from supabase database 
                    // map supabase result to dto
                    var dtos = res.Models.Select(m => new DTOs.User
                    {
                        Id = m.Id,
                        TeamName = m.TeamName,
                        Email = m.Email,
                        RosterSettingsId = m.RosterSettingsId,
                        ScoringSettingsId = m.ScoringSettingsId,
                        TokensLeft = m.TokensLeft,
                    }).ToList();
                    // Return as anonymous object to ensure clean serialization
                    return new JsonResult(dtos);
                }
                case "players":
                {
                    var res = await _supabase.From<Player>().Get();
                    var dtos = res.Models.Select(m => new DTOs.Player
                    {
                        Id = m.Id,
                        Name = m.Name,
                        HeadshotUrl = m.HeadshotUrl,
                        Position = m.Position,
                        Status = m.Status,
                        StatusDescription = m.StatusDescription,
                        TeamId = m.TeamId,
                        SeasonStatsId = m.SeasonStatsId
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "player_stats":
                {
                    var res = await _supabase.From<PlayerStat>().Get();
                    var dtos = res.Models.Select(m => new DTOs.PlayerStat
                    {
                        Completions = m.Completions,
                        PassingAttempts = m.PassingAttempts,
                        PassingYards = m.PassingYards,
                        PassingTds = m.PassingTds,
                        InterceptionsAgainst = m.InterceptionsAgainst,
                        SacksAgainst = m.SacksAgainst,
                        FumblesAgainst = m.FumblesAgainst,
                        PassingFirstDowns = m.PassingFirstDowns,
                        PassingEpa = m.PassingEpa,
                        Carries = m.Carries,
                        RushingYards = m.RushingYards,
                        RushingTds = m.RushingTds,
                        RushingFirstDowns = m.RushingFirstDowns,
                        RushingEpa = m.RushingEpa,
                        Receptions = m.Receptions,
                        Targets = m.Targets,
                        ReceivingYards = m.ReceivingYards,
                        ReceivingTds = m.ReceivingTds,
                        ReceivingFirstDowns = m.ReceivingFirstDowns,
                        ReceivingEpa = m.ReceivingEpa,
                        FgMadeList = m.FgMadeList,
                        FgMissedList = m.FgMissedList,
                        FgBlockedList = m.FgBlockedList,
                        PadAttempts = m.PadAttempts,
                        PatPercent = m.PatPercent,
                        FantasyPoints = m.FantasyPoints,
                        FantasyPointsPpr = m.FantasyPointsPpr
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "teams":
                {
                    var res = await _supabase.From<Team>().Get();
                    var dtos = res.Models.Select(m => new DTOs.Team
                    {
                        Id = m.Id,
                        Name = m.Name,
                        OffensiveStatsId = m.OffensiveStatsId,
                        DefensiveStatsId = m.DefensiveStatsId
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "team_members":
                {
                    var res = await _supabase.From<TeamMember>().Get();
                    var dtos = res.Models.Select(m => new DTOs.TeamMember
                    {
                        UserId = m.UserId,
                        PlayerId = m.PlayerId
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "weekly_player_stats":
                {
                    var res = await _supabase.From<WeeklyPlayerStat>().Get();
                    var dtos = res.Models.Select(m => new DTOs.WeeklyPlayerStat
                    {
                        Id = m.Id,
                        Week = m.Week,
                        PlayerId = m.PlayerId,
                        PlayerStatsId = m.PlayerStatsId
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "scoring_settings":
                {
                    var res = await _supabase.From<ScoringSetting>().Get();
                    var dtos = res.Models.Select(m => new DTOs.ScoringSetting
                    {
                        Id = m.Id,
                        PointsPerTd = m.PointsPerTd,
                        PointsPerReception = m.PointsPerReception,
                        PointsPerPassingYard = m.PointsPerPassingYard,
                        PointsPerRushingYard = m.PointsPerRushingYard,
                        PointsPerReceptionYard = m.PointsPerReceptionYard
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "roster_settings":
                {
                    var res = await _supabase.From<RosterSetting>().Get();
                    var dtos = res.Models.Select(m => new DTOs.RosterSetting
                    {
                        Id = m.Id,
                        QbCount = m.QbCount,
                        RbCount = m.RbCount,
                        TeCount = m.TeCount,
                        WrCount = m.WrCount,
                        KCount = m.KCount,
                        DefCount = m.DefCount,
                        FlexCount = m.FlexCount,
                        BenchCount = m.BenchCount,
                        IrCount = m.IrCount
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "team_offensive_stats":
                {
                    var res = await _supabase.From<TeamOffensiveStat>().Get();
                    var dtos = res.Models.Select(m => new DTOs.TeamOffensiveStat
                    {
                        Id = m.Id,
                        Completions = m.Completions,
                        Attempts = m.Attempts,
                        PassingYards = m.PassingYards,
                        PassingTds = m.PassingTds,
                        PassingInterceptions = m.PassingInterceptions,
                        SacksAgainst = m.SacksAgainst,
                        FumblesAgainst = m.FumblesAgainst,
                        Carries = m.Carries,
                        RushingYards = m.RushingYards,
                        RushingTds = m.RushingTds,
                        Receptions = m.Receptions,
                        Targets = m.Targets,
                        ReceivingYards = m.ReceivingYards,
                        ReceivingTds = m.ReceivingTds
                    }).ToList();
                    return new JsonResult(dtos);
                }
                case "team_defensive_stats":
                {
                    var res = await _supabase.From<TeamDefensiveStat>().Get();
                    var dtos = res.Models.Select(m => new DTOs.TeamDefensiveStat
                    {
                        Id = m.Id,
                        TacklesForLoss = m.TacklesForLoss,
                        TacklesForLossYards = m.TacklesForLossYards,
                        FumblesFor = m.FumblesFor,
                        SacksFor = m.SacksFor,
                        SackYardsFor = m.SackYardsFor,
                        InterceptionsFor = m.InterceptionsFor,
                        InterceptionYardsFor = m.InterceptionYardsFor,
                        DefTds = m.DefTds,
                        Safeties = m.Safeties,
                        PassDefended = m.PassDefended
                    }).ToList();
                    return new JsonResult(dtos);
                }
                default:
                    return BadRequest(new { error = "Unhandled table" });
            }
        }
        catch (Exception ex)
        {
           
            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}