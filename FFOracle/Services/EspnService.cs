using FFOracle.DTOs.Responses;
using FFOracle.Models.Supabase;
using Supabase;

namespace FFOracle.Services;

public class EspnService
{
    private const string GET_IDS_BASE_URL = "https://sports.core.api.espn.com/v2/sports/football/leagues/nfl/athletes";
    private const string GET_ARTICLES_BASE_URL = "https://site.api.espn.com/apis/fantasy/v2/games/ffl/news/players";
    private readonly HashSet<string> OFFENSIVE_POSITIONS = new(new[] { "QB", "RB", "WR", "TE", "K" });

    // Concurrency limit for ESPN detail calls
    private const int MAX_PARALLEL = 32;
    private readonly SemaphoreSlim _throttle = new(MAX_PARALLEL);

    private readonly Client _supabaseClient;
    private readonly HttpClient _httpClient;

    public EspnService(Client supabaseClient, HttpClient httpClient)
    {
        _supabaseClient = supabaseClient;
        _httpClient = httpClient;
    }

    public async Task SyncAllPlayersAsync()
    {
        // Load all local players once
        var localPlayers = await _supabaseClient
            .From<Player>()
            .Range(0, 9999)
            .Get();

        var localLookup = localPlayers.Models
            .ToDictionary(
                p => $"{p.Name}|{p.Position}",
                p => p.Id,
                StringComparer.OrdinalIgnoreCase
            );

        int page = 1;

        while (true)
        {
            var url = $"{GET_IDS_BASE_URL}?limit=1000&active=true&page={page}";
            var response = await _httpClient.GetFromJsonAsync<EspnAthleteListResponse>(url);

            if (response == null || response.Items.Count == 0)
                break;

            // Fetch all ESPN detail pages with concurrency cap
            var detailTasks = response.Items.Select(async item =>
            {
                await _throttle.WaitAsync();
                try
                {
                    return await _httpClient.GetFromJsonAsync<EspnAthleteDetailResponse>(item.Ref);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    _throttle.Release();
                }
            }).ToArray();

            var details = await Task.WhenAll(detailTasks);

            foreach (var detail in details)
            {
                if (detail?.Position?.Abbreviation == null ||
                    !OFFENSIVE_POSITIONS.Contains(detail.Position.Abbreviation))
                    continue;

                var key = $"{detail.FullName}|{detail.Position.Abbreviation}";

                if (!localLookup.TryGetValue(key, out var localPlayerId))
                    continue;

                // Upsert row-by-row
                var payload = new PlayerEspnId
                {
                    PlayerId = localPlayerId,
                    EspnId = detail.Id
                };

                await _supabaseClient
                    .From<PlayerEspnId>()
                    .Upsert(payload);
            }

            page++;
        }
    }

    public async Task<List<EspnNewsItem>> GetArticlesForPlayerAsync(string playerId)
    {
        // get espn id for player
        var espnIdRecord = await _supabaseClient
            .From<PlayerEspnId>()
            .Where(p => p.PlayerId == playerId)
            .Single();

        if (espnIdRecord == null)
        {
            return [];
        }

        // fetch articles from espn
        var url = $"{GET_ARTICLES_BASE_URL}?limit=50&playerId={espnIdRecord.EspnId}";
        var response = await _httpClient.GetFromJsonAsync<EspnNewsResponse>(url);
        List<EspnNewsItem> articles = response?.Feed ?? [];

        // attach player id to each article
        foreach (var article in articles)
        {
            article.LocalPlayerId = playerId;
        }

        return articles;
    }
}
