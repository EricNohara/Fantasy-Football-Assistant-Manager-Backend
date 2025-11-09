using Fantasy_Football_Assistant_Manager.Models.Supabase;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Responses;
using static Supabase.Postgrest.Constants;

//A controller to retrieve all data specific to a certain user
//Based on the SupeBaseController code used for our test app

namespace Fantasy_Football_Assistant_Manager.Controllers.SupaBaseControllers;

[ApiController]
[Route("api/[controller]")]
public class GetUserDataController : ControllerBase
{
    private readonly Client _supabase;

    public GetUserDataController(Client supabase)
    {
        _supabase = supabase;
    }

    // Read-only endpoint to fetch all user data: account info, settings, team
    [HttpGet]
    public async Task<IActionResult> GetUserData()
    {
        try
        {
            // get the token from the Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Missing or invalid token");
            }
            var accessToken = authHeader.Substring("Bearer ".Length);
            // verify the token with Supabase
            var user = await _supabase.Auth.GetUser(accessToken);
            if (user == null)
            {
                return Unauthorized("Invalid token");
            }
            var userId = Guid.Parse(user.Id);
            if (userId == Guid.Empty)
            {
                return Unauthorized("Invalid token");
            }

       
            // fetch user data from supabase database 
            var userRes = await _supabase.From<User>().Where(x => x.Id == userId).Get();

            // map supabase result to dto
            var m = userRes.Model;

            var userDTO = new DTOs.User();
            userDTO.Id = m.Id;
            userDTO.TeamName = m.TeamName;
            userDTO.Email = m.Email;
            userDTO.TokensLeft = m.TokensLeft;
            //no need to send the settings entry IDs since these will be queried separately
            
            // fetch score settings and map to dto
            var scoreRes = await _supabase.From<ScoringSetting>().Where(x => x.Id == m.ScoringSettingsId).Get();
            var s = scoreRes.Model;
            var scoreDTO = new DTOs.ScoringSetting();
            scoreDTO.PointsPerReception = s.PointsPerReception;
            scoreDTO.PointsPerReceptionYard = s.PointsPerReceptionYard;
            scoreDTO.PointsPerRushingYard = s.PointsPerRushingYard;
            scoreDTO.PointsPerPassingYard = s.PointsPerPassingYard;
            scoreDTO.PointsPerTd = s.PointsPerTd;

            //fetch roster settings and map to dto
            var rosterRes = await _supabase.From<RosterSetting>().Where(x => x.Id == m.RosterSettingsId).Get();
            var r = rosterRes.Model;
            var rosterDTO = new DTOs.RosterSetting();
            rosterDTO.KCount = r.KCount;
            rosterDTO.WrCount = r.WrCount;
            rosterDTO.IrCount = r.IrCount;
            rosterDTO.QbCount = r.QbCount;
            rosterDTO.RbCount = r.RbCount;
            rosterDTO.BenchCount = r.BenchCount;
            rosterDTO.DefCount = r.DefCount;
            rosterDTO.FlexCount = r.FlexCount;
            rosterDTO.TeCount = r.TeCount;

            //fetch list of players on user's team and map to dtos
            //start by getting all picked player ids
            var teamIDs = await _supabase.From<TeamMember>().Where(x => x.UserId == m.Id).Get();
            var playerIds = teamIDs.Models.Select(up => up.PlayerId).Distinct().ToList();   //list of player ids
            List<DTOs.Player> memberDTOs = new List<DTOs.Player>();   //new list of player dtos
            if (playerIds.Any())    //as long as this user has picked players...
            {
                //make a new query on the player table for all players with an id in the id list
                var memberRes = await _supabase
                    .From<Player>()
                    .Filter("Id", Operator.In, $"({string.Join(",", playerIds)})")
                    .Get();

                memberDTOs = memberRes.Models.Select(m => new DTOs.Player
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
            }

            // Return as anonymous object with Ok
            var result = new
            {
                userDTO,
                scoreDTO,
                rosterDTO,
                memberDTOs
            };
            return Ok(result);
            

            




           
            //    case "players":
            //        {
            //            var res = await _supabase.From<Player>().Get();
            //            var dtos = res.Models.Select(m => new DTOs.Player
            //            {
            //                Id = m.Id,
            //                Name = m.Name,
            //                HeadshotUrl = m.HeadshotUrl,
            //                Position = m.Position,
            //                Status = m.Status,
            //                StatusDescription = m.StatusDescription,
            //                TeamId = m.TeamId,
            //                SeasonStatsId = m.SeasonStatsId
            //            }).ToList();
            //            return new JsonResult(dtos);
            //        }
            //    case "player_stats":
            //        {
            //            var res = await _supabase.From<PlayerStat>().Get();
            //            var dtos = res.Models.Select(m => new DTOs.PlayerStat
            //            {
            //                Completions = m.Completions,
            //                PassingAttempts = m.PassingAttempts,
            //                PassingYards = m.PassingYards,
            //                PassingTds = m.PassingTds,
            //                InterceptionsAgainst = m.InterceptionsAgainst,
            //                SacksAgainst = m.SacksAgainst,
            //                FumblesAgainst = m.FumblesAgainst,
            //                PassingFirstDowns = m.PassingFirstDowns,
            //                PassingEpa = m.PassingEpa,
            //                Carries = m.Carries,
            //                RushingYards = m.RushingYards,
            //                RushingTds = m.RushingTds,
            //                RushingFirstDowns = m.RushingFirstDowns,
            //                RushingEpa = m.RushingEpa,
            //                Receptions = m.Receptions,
            //                Targets = m.Targets,
            //                ReceivingYards = m.ReceivingYards,
            //                ReceivingTds = m.ReceivingTds,
            //                ReceivingFirstDowns = m.ReceivingFirstDowns,
            //                ReceivingEpa = m.ReceivingEpa,
            //                FgMadeList = m.FgMadeList,
            //                FgMissedList = m.FgMissedList,
            //                FgBlockedList = m.FgBlockedList,
            //                PadAttempts = m.PadAttempts,
            //                PatPercent = m.PatPercent,
            //                FantasyPoints = m.FantasyPoints,
            //                FantasyPointsPpr = m.FantasyPointsPpr
            //            }).ToList();
            //            return new JsonResult(dtos);
            //        }
            //    case "teams":
            //        {
            //            var res = await _supabase.From<Team>().Get();
            //            var dtos = res.Models.Select(m => new DTOs.Team
            //            {
            //                Id = m.Id,
            //                Name = m.Name,
            //                OffensiveStatsId = m.OffensiveStatsId,
            //                DefensiveStatsId = m.DefensiveStatsId
            //            }).ToList();
            //            return new JsonResult(dtos);
            //        }
                
            //    case "weekly_player_stats":
            //        {
            //            var res = await _supabase.From<WeeklyPlayerStat>().Get();
            //            var dtos = res.Models.Select(m => new DTOs.WeeklyPlayerStat
            //            {
            //                Id = m.Id,
            //                Week = m.Week,
            //                PlayerId = m.PlayerId,
            //                PlayerStatsId = m.PlayerStatsId
            //            }).ToList();
            //            return new JsonResult(dtos);
            //        }
            //    case "scoring_settings":
            //        {
            //            var res = await _supabase.From<ScoringSetting>().Get();
            //            var dtos = res.Models.Select(m => new DTOs.ScoringSetting
            //            {
            //                Id = m.Id,
            //                PointsPerTd = m.PointsPerTd,
            //                PointsPerReception = m.PointsPerReception,
            //                PointsPerPassingYard = m.PointsPerPassingYard,
            //                PointsPerRushingYard = m.PointsPerRushingYard,
            //                PointsPerReceptionYard = m.PointsPerReceptionYard
            //            }).ToList();
            //            return new JsonResult(dtos);
            //        }
            //    case "roster_settings":
            //        {
            //            var res = await _supabase.From<RosterSetting>().Get();
            //            var dtos = res.Models.Select(m => new DTOs.RosterSetting
            //            {
            //                Id = m.Id,
            //                QbCount = m.QbCount,
            //                RbCount = m.RbCount,
            //                TeCount = m.TeCount,
            //                WrCount = m.WrCount,
            //                KCount = m.KCount,
            //                DefCount = m.DefCount,
            //                FlexCount = m.FlexCount,
            //                BenchCount = m.BenchCount,
            //                IrCount = m.IrCount
            //            }).ToList();
            //            return new JsonResult(dtos);
            //        }
                
            //    default:
            //        return BadRequest(new { error = "Unhandled table" });
            //}
        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}
