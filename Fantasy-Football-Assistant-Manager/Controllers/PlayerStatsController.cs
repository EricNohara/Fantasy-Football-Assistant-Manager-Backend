using Fantasy_Football_Assistant_Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fantasy_Football_Assistant_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerStatsController: ControllerBase
{
    private readonly NflVerseService _nflVerseService;

    // injects NflVerseService via dependency injection
    public PlayerStatsController(NflVerseService nflVerseService)
    {
        _nflVerseService = nflVerseService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<Models.PlayerStat>>> GetAllPlayerStats()
    {
        // retrieve ALL player stats from the NflVerse service and map it to simple DTO for JSON serialization
        try
        {
            // fetch all player stats via the service
            var stats = await _nflVerseService.GetAllPlayerStatsAsync();

            // map from supabase model to PlayerStat DTO in order to serialize it as JSON
            // map 0 values to null for efficient database storage
            // need this step since you cannot directly serialize a supabase model
            var dtoList = stats.Select(p => new DTOs.PlayerStat
            {
                Completions = p.Completions == 0 ? null : p.Completions,
                PassingAttempts = p.PassingAttempts == 0 ? null : p.PassingAttempts,
                PassingYards = p.PassingYards == 0 ? null : p.PassingYards,
                PassingTds = p.PassingTds == 0 ? null : p.PassingTds,
                InterceptionsAgainst = p.InterceptionsAgainst == 0 ? null : p.InterceptionsAgainst,
                SacksAgainst = p.SacksAgainst == 0 ? null : p.SacksAgainst,
                FumblesAgainst = p.FumblesAgainst == 0 ? null : p.FumblesAgainst,
                PassingFirstDowns = p.PassingFirstDowns == 0 ? null : p.PassingFirstDowns,
                PassingEpa = p.PassingEpa == 0 ? null : p.PassingEpa,
                Carries = p.Carries == 0 ? null : p.Carries,
                RushingYards = p.RushingYards == 0 ? null : p.RushingYards,
                RushingTds = p.RushingTds == 0 ? null : p.RushingTds,
                RushingFirstDowns = p.RushingFirstDowns == 0 ? null : p.RushingFirstDowns,
                RushingEpa = p.RushingEpa == 0 ? null : p.RushingEpa,
                Receptions = p.Receptions == 0 ? null : p.Receptions,
                Targets = p.Targets == 0 ? null : p.Targets,
                ReceivingYards = p.ReceivingYards == 0 ? null : p.ReceivingYards,
                ReceivingTds = p.ReceivingTds == 0 ? null : p.ReceivingTds,
                ReceivingFirstDowns = p.ReceivingFirstDowns == 0 ? null : p.ReceivingFirstDowns,
                ReceivingEpa = p.ReceivingEpa == 0 ? null : p.ReceivingEpa,
                FgMadeList = p.FgMadeList,
                FgMissedList = p.FgMissedList,
                FgBlockedList = p.FgBlockedList,
                PadAttempts = p.PadAttempts == 0 ? null : p.PadAttempts,
                PatPercent = p.PatPercent == 0 ? null : p.PatPercent,
                FantasyPoints = p.FantasyPoints == 0 ? null : p.FantasyPoints,
                FantasyPointsPpr = p.FantasyPointsPpr == 0 ? null : p.FantasyPointsPpr
            }).ToList();

            return Ok(dtoList);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching player stats: {ex.Message}");
        }
    }
}
