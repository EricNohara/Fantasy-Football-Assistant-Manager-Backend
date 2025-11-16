//using Fantasy_Football_Assistant_Manager.DTOs;
using FFOracle.Models.Supabase;
using FFOracle.Services;
using FFOracle.Utils;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Responses;
using System.Reflection;
using static Supabase.Postgrest.Constants;

//A controller to retrieve all data specific to a certain user
//Based on the SupeBaseController code used for our test app

namespace FFOracle.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class GetUserDataController : ControllerBase
{
    private readonly Client _supabase;
    private readonly SupabaseAuthService _authService;

    public GetUserDataController(Client supabase, SupabaseAuthService authService)
    {
        _supabase = supabase;
        _authService = authService;
    }

    // Read-only endpoint to fetch all user data: account info, settings, team
    [HttpGet]
    public async Task<IActionResult> GetUserData()
    {
        try
        {
            // get the token from the Authorization header
            var userId = await _authService.AuthorizeUser(Request);
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
            userDTO.Fullname = userData.Fullname;
            userDTO.Email = userData.Email;
            userDTO.TokensLeft = userData.TokensLeft;

            //fetch each of this user's leagues and their associated data, then map them to
            // DTOs

            //first, obtain list of this user's leagues
            var leaguesRes = await _supabase
                .From<UserLeague>()
                .Where(x => x.UserID == userData.Id)
                .Get();
            var leagues = leaguesRes.Models.ToList();

            //for each league, obtain the league info and store in DTO list
            var leaguesWithInfo = new List<DTOs.UserLeagueWithSettingsAndPlayers>();
            //obtain the lists of roster settings and scoring settings in advance to
            // minimize supabase queries
            var scoreSettingIds = leagues.Select(x => x.ScoringSettingsID).ToList(); //used to narrow down settings pulled from supabase
            var rosterSettingIds = leagues.Select(x => x.RosterSettingsID).ToList(); //used for same purpose as scoreSettingIds
            var scoreSettingsRes = await _supabase
                .From<ScoringSetting>()
                .Filter(s => s.Id, Operator.In, scoreSettingIds)
                .Get();
            var scoreSettings = scoreSettingsRes.Models.ToList();
            var rosterSettingsRes = await _supabase
                .From<RosterSetting>()
                .Filter(s => s.Id, Operator.In, rosterSettingIds)
                .Get();
            var rosterSettings = rosterSettingsRes.Models.ToList();
            foreach ( var league in leagues)
            {
                var leagueDTO = new DTOs.UserLeagueWithSettingsAndPlayers();
                leagueDTO.League = new DTOs.UserLeague
                {
                    Id = league.Id,
                    Name = league.Name,
                    UserId = league.UserID,
                    ScoringSettingsId = league.ScoringSettingsID,
                    RosterSettingsId = league.RosterSettingsID
                };
                //get roster setting
                var rosterSetting = rosterSettings
                    .Where(s => s.Id == league.RosterSettingsID)
                    .Single();
                leagueDTO.RosterSetting = new DTOs.RosterSetting
                {
                    KCount = rosterSetting.KCount,
                    WrCount = rosterSetting.WrCount,
                    IrCount = rosterSetting.IrCount,
                    QbCount = rosterSetting.QbCount,
                    RbCount = rosterSetting.RbCount,
                    BenchCount = rosterSetting.BenchCount,
                    DefCount = rosterSetting.DefCount,
                    FlexCount = rosterSetting.FlexCount,
                    TeCount = rosterSetting.TeCount
                };
                //get score setting
                var scoreSetting = scoreSettings
                    .Where(s => s.Id == league.ScoringSettingsID)
                    .Single();
                leagueDTO.ScoringSetting = new DTOs.ScoringSetting
                {
                    PointsPerReception = scoreSetting.PointsPerReception,
                    PointsPerReceptionYard = scoreSetting.PointsPerReceptionYard,
                    PointsPerRushingYard = scoreSetting.PointsPerRushingYard,
                    PointsPerPassingYard = scoreSetting.PointsPerPassingYard,
                    PointsPerTd = scoreSetting.PointsPerTd
                };
                //get list of players in this league
                var membersRes = await _supabase    //get the LeagueMember objects first
                    .From<LeagueMember>()
                    .Where(m => m.LeagueId == league.Id)
                    .Get();
                var members = membersRes.Models.ToList();
                var memberIds = members.Select(m => m.PlayerId).ToList();
                var playersRes = await _supabase    //use LeagueMember player ids to get the players
                    .From<Player>()
                    .Filter(p => p.Id, Operator.In, memberIds)
                    .Get();
                var players = playersRes.Models.ToList();
                //use helper function to link each player to their stats
                var playerWithStatsDTOs = await Helper.GetPlayersWithStats(players, _supabase);
                //Link each player to their picked status for this league
                var userLeagueMemberDTOs = members.Select(m =>
                {
                    var ulm = new DTOs.UserLeagueMember
                    {
                        Picked = m.Picked,
                        Player = playerWithStatsDTOs
                            .Where(p => p.Player.Id == m.PlayerId)
                            .Single()
                    };
                    return ulm;
                }).ToList();
                leagueDTO.Players = userLeagueMemberDTOs;
                leaguesWithInfo.Add(leagueDTO); //finally, add to the list of leagues
            }

            //Return the user info with the list of that user's leagues
            var result = new
            {
                userDTO,
                leaguesWithInfo
            };

            return Ok(result);
        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Error fetching data from Supabase", details = ex.Message });
        }
    }
}