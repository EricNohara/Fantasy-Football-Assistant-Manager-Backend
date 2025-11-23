using FFOracle.Models.Supabase;
using FFOracle.DTOs.Responses;
using Supabase;

namespace FFOracle.Services;

public class EspnService
{
    private const string BASE_URL = "https://sports.core.api.espn.com/v2/sports/football/leagues/nfl/athletes";
    private readonly HashSet<string> OFFENSIVE_POSITIONS = new(new[] { "QB", "RB", "WR", "TE", "K" });

    private readonly Client _supabaseClient;
    private readonly HttpClient _httpClient;

    public EspnService(Client supabaseClient, HttpClient httpClient)
    {
        _supabaseClient = supabaseClient;
        _httpClient = httpClient;
    }

    public async Task SyncAllPlayersAsync()
    {
        int page = 1;
        while(true)
        {
            var url = $"{BASE_URL}?limit=100&active=true&page={page}";
            var response = await _httpClient.GetFromJsonAsync<EspnAthleteListResponse>(url) ?? new EspnAthleteListResponse();

            if (response.Items.Count == 0)
            {
                // no more players
                break;
            }

            foreach (var item in response.Items)
            {
                var detail = await _httpClient.GetFromJsonAsync<EspnAthleteDetailResponse>(item.Ref);

                if (detail?.Position?.Abbreviation == null ||
                    !OFFENSIVE_POSITIONS.Contains(detail.Position.Abbreviation) ||
                    string.IsNullOrWhiteSpace(detail.Id)
                )
                {
                    continue;
                }

                var mappedPlayerId = await MapEspnToLocalPlayer(detail.FullName, detail.Position.Abbreviation);

                if (mappedPlayerId == null)
                {
                    continue;
                }

                // upsert player espn id in to player_espn_ids table
                var payload = new PlayerEspnId
                {
                    PlayerId = mappedPlayerId,
                    EspnId = detail.Id
                };

                await _supabaseClient
                    .From<PlayerEspnId>()
                    .Upsert(payload);
            }

            page++;
        }
    }

    private async Task<string?> MapEspnToLocalPlayer(string fullName, string position)
    {
        var result = await _supabaseClient
            .From<Player>()
            .Where(x => x.Name == fullName && x.Position == position)
            .Single();

        return result?.Id;
    }
}
