using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Fantasy_Football_Assistant_Manager.Services;
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

    // helper function for data cleaning
    private static int? NullIfZero(int? value) => value == 0 ? null : value;
    private static float? NullIfZero(float? value) => value == 0f ? null : value;

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
                    Completions = NullIfZero(stat.Completions),
                    Attempts = NullIfZero(stat.Attempts),
                    PassingYards = NullIfZero(stat.PassingYards),
                    PassingTds = NullIfZero(stat.PassingTds),
                    PassingInterceptions = NullIfZero(stat.PassingInterceptions),
                    SacksAgainst = NullIfZero(stat.SacksAgainst),
                    FumblesAgainst = NullIfZero(stat.FumblesAgainst),
                    Carries = NullIfZero(stat.Carries),
                    RushingYards = NullIfZero(stat.RushingYards),
                    RushingTds = NullIfZero(stat.RushingTds),
                    Receptions = NullIfZero(stat.Receptions),
                    Targets = NullIfZero(stat.Targets),
                    ReceivingYards = NullIfZero(stat.ReceivingYards),
                    ReceivingTds = NullIfZero(stat.ReceivingTds)
                };

                offensiveStats.Add(offensiveStat);

                // parse out the defensive stats and add to list
                var defensiveStat = new TeamDefensiveStat
                {
                    Id = defensiveId,
                    TacklesForLoss = NullIfZero(stat.TacklesForLoss),
                    TacklesForLossYards = NullIfZero(stat.TacklesForLossYards),
                    FumblesFor = NullIfZero(stat.FumblesFor),
                    SacksFor = NullIfZero(stat.SacksFor),
                    SackYardsFor = NullIfZero(stat.SackYardsFor),
                    InterceptionsFor = NullIfZero(stat.InterceptionsFor),
                    InterceptionYardsFor = NullIfZero(stat.InterceptionYardsFor),
                    DefTds = NullIfZero(stat.DefTds),
                    Safeties = NullIfZero(stat.Safeties),
                    PassDefended = NullIfZero(stat.PassDefended)
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
