//using Fantasy_Football_Assistant_Manager.DTOs;
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
            var userData = userRes.Model;

            var userDTO = new DTOs.User();
            userDTO.Id = userData.Id;
            userDTO.TeamName = userData.TeamName;
            userDTO.Email = userData.Email;
            userDTO.TokensLeft = userData.TokensLeft;
            //no need to send the settings entry IDs since these will be queried separately
            
            // fetch score settings and map to dto
            var scoreRes = await _supabase.From<ScoringSetting>().Where(x => x.Id == userData.ScoringSettingsId).Get();
            var s = scoreRes.Model;
            var scoreDTO = new DTOs.ScoringSetting();
            scoreDTO.PointsPerReception = s.PointsPerReception;
            scoreDTO.PointsPerReceptionYard = s.PointsPerReceptionYard;
            scoreDTO.PointsPerRushingYard = s.PointsPerRushingYard;
            scoreDTO.PointsPerPassingYard = s.PointsPerPassingYard;
            scoreDTO.PointsPerTd = s.PointsPerTd;

            //fetch roster settings and map to dto
            var rosterRes = await _supabase.From<RosterSetting>().Where(x => x.Id == userData.RosterSettingsId).Get();
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

            //fetch team member data and map to list of DTOs
            //start by fetching the list of TeamMember objects for this user
            var membersRes = await _supabase
                .From<Models.Supabase.TeamMember>()
                .Where(x => x.UserId == userData.Id)
                .Get();
            var members = membersRes.Models;
            //Extract a list of just the team member IDs and use it to fetch a list of
            // player objects for the players on this user's team.
            var memberIDs = members
                .Select(m => m.PlayerId)
                .ToList();
            var playersRes = await _supabase
                .From<Player>()
                .Filter(p => p.Id, Operator.In, memberIDs)
                .Get();
            var players = playersRes.Models;
            //Use a helper method to obtain a list of players with associated stats
            var playerWithStatsDTOs = await Helper.GetPlayersWithStats(players: players, _supabase: _supabase);

            //Combine the picked fields of the members list with the playerWithStats DTOs
            var userTeamMemberDTOs = members.Select(m =>
            {
                var utm = new DTOs.UserTeamMember
                {
                    Picked = m.Picked,
                    Player = playerWithStatsDTOs
                        .Where(p => p.Player.Id == m.PlayerId.ToString())
                        .Single()
                };
                return utm;
            }).ToList();

            //return all user data
            var result = new
            {
                userDTO,
                scoreDTO,
                rosterDTO,
                userTeamMemberDTOs
            };
            return Ok(result);      
        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}
