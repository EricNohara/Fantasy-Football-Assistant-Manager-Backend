using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Services;
using Fantasy_Football_Assistant_Manager.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace Fantasy_Football_Assistant_Manager.Controllers;

public class TeamSeasonStatController : ControllerBase
{
    private readonly NflVerseService _nflVerseService;
    private readonly Client _supabase;

    public TeamSeasonStatController(NflVerseService nflVerseService, Client supabase)
    {
        _nflVerseService = nflVerseService;
        _supabase = supabase;
    }

    [HttpPost("all")]
    public async Task<IActionResult> PostAll()
    {
        try
        {
            // fetch team stats using service
            var stats = await _nflVerseService.GetAllTeamSeasonStatsAsync();
            if (stats == null || !stats.Any())
            {
                return BadRequest("No team stats found to insert.");
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
                return NotFound("No teams found in the database");
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

            return Ok(new { message = "Team stats inserted successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error populating team stats: {ex.Message}");
        }
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            // delete all offensive stats
            await _supabase
                .From<TeamOffensiveStat>()
                .Where(s => s.Id != null)
                .Delete();

            // delete all defensive stats
            await _supabase
                .From<TeamDefensiveStat>()
                .Where(s => s.Id != null)
                .Delete();

            // the foreign keys set to null automatically in teams table
            return Ok(new { message = "Successfully deleted all team stats" });
        } catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting all team stats: {ex.Message}");
        }
    }
}
