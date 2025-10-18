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
            // need this step since you cannot directly serialize a supabase model
            var dtoList = stats.Select(p => new DTOs.PlayerStat
            {
                Completions = p.Completions,
                PassingAttempts = p.PassingAttempts,
                PassingYards = p.PassingYards,
                PassingTds = p.PassingTds,
                InterceptionsAgainst = p.InterceptionsAgainst,
                SacksAgainst = p.SacksAgainst,
                FumblesAgainst = p.FumblesAgainst,
                PassingFirstDowns = p.PassingFirstDowns,
                PassingEpa = p.PassingEpa,
                Carries = p.Carries,
                RushingYards = p.RushingYards,
                RushingTds = p.RushingTds,
                RushingFirstDowns = p.RushingFirstDowns,
                RushingEpa = p.RushingEpa,
                Receptions = p.Receptions,
                Targets = p.Targets,
                ReceivingYards = p.ReceivingYards,
                ReceivingTds = p.ReceivingTds,
                ReceivingFirstDowns = p.ReceivingFirstDowns,
                ReceivingEpa = p.ReceivingEpa,
                FgMadeList = p.FgMadeList,
                FgMissedList = p.FgMissedList,
                FgBlockedList = p.FgBlockedList,
                PadAttempts = p.PadAttempts,
                PatPercent = p.PatPercent,
                FantasyPoints = p.FantasyPoints,
                FantasyPointsPpr = p.FantasyPointsPpr
            }).ToList();

            return Ok(dtoList);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching player stats: {ex.Message}");
        }
    }
}
