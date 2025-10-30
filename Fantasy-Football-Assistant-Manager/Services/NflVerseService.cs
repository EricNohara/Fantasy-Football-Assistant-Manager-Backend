using CsvHelper;
using CsvHelper.Configuration;
using Fantasy_Football_Assistant_Manager.Mappings;
using Fantasy_Football_Assistant_Manager.Models.Csv;
using Fantasy_Football_Assistant_Manager.Models.Supabase;
using System.Globalization;
using System.IO.Compression;

namespace Fantasy_Football_Assistant_Manager.Services;

public class NflVerseService
{
    private readonly HttpClient _httpClient;

    // nflverse urls
    private const string SEASON_PLAYER_STATS_URL = "https://github.com/nflverse/nflverse-data/releases/download/stats_player/stats_player_reg_2025.csv.gz";
    private const string WEEKLY_PLAYER_STATS_URL = "https://github.com/nflverse/nflverse-data/releases/download/stats_player/stats_player_week_2025.csv.gz";
    private const string PLAYER_INFORMATION_URL = "https://github.com/nflverse/nflverse-data/releases/download/players/players.csv.gz";

    private const string SEASON_TEAM_STATS_URL = "https://github.com/nflverse/nflverse-data/releases/download/stats_team/stats_team_regpost_2025.csv.gz";
    private const string TEAM_DATA_URL = "https://github.com/nflverse/nflverse-data/releases/download/teams/teams_colors_logos.csv.gz";

    // defensive positions (we only store offensive players)
    private HashSet<string> DEFENSIVE_POSITIONS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "DL",  // Defensive Lineman (general)
        "DE",  // Defensive End
        "DT",  // Defensive Tackle
        "NT",  // Nose Tackle
        "LB",  // Linebacker (general)
        "ILB", // Inside Linebacker
        "MLB", // Middle Linebacker
        "OLB", // Outside Linebacker
        "DB",  // Defensive Back (general)
        "CB",  // Cornerback
        "S",   // Safety (general)
        "FS",  // Free Safety
        "SS"   // Strong Safety
    };

    // current valid NFL teams
    private HashSet<string> CURRENT_TEAM_IDS_IN_NFL = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
    {
        "ARI", // Arizona Cardinals
        "ATL", // Atlanta Falcons
        "BAL", // Baltimore Ravens
        "BUF", // Buffalo Bills
        "CAR", // Carolina Panthers
        "CHI", // Chicago Bears
        "CIN", // Cincinnati Bengals
        "CLE", // Cleveland Browns
        "DAL", // Dallas Cowboys
        "DEN", // Denver Broncos
        "DET", // Detroit Lions
        "GB",  // Green Bay Packers
        "HOU", // Houston Texans
        "IND", // Indianapolis Colts
        "JAX", // Jacksonville Jaguars
        "KC",  // Kansas City Chiefs
        "LA",  // Los Angeles Rams
        "LAC", // Los Angeles Chargers
        "LV",  // Las Vegas Raiders
        "MIA", // Miami Dolphins
        "MIN", // Minnesota Vikings
        "NE",  // New England Patriots
        "NO",  // New Orleans Saints
        "NYG", // New York Giants
        "NYJ", // New York Jets
        "PHI", // Philadelphia Eagles
        "PIT", // Pittsburgh Steelers
        "SEA", // Seattle Seahawks
        "SF",  // San Francisco 49ers
        "TB",  // Tampa Bay Buccaneers
        "TEN", // Tennessee Titans
        "WAS"  // Washington Commanders
    };

    // current season
    private const int CURRENT_SEASON = 2025;

    public NflVerseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // private helper for loading and parsing CSV files
    // takes in the url to fetch, an optional filter function, and a class map for name mappings
    private async Task<List<T>> LoadCsvDataAsync<T, TMap>(string url, Func<T, bool>? filter = null) where TMap : ClassMap {
        // get the inputted CSV file
        using var res = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        res.EnsureSuccessStatusCode();

        // read the contents of the CSV as a stream
        await using var stream = await res.Content.ReadAsStreamAsync();

        // wrap in GZipStream for decompression
        await using var decompressedStream = new GZipStream(stream, CompressionMode.Decompress);

        // set up the CsvReader using the decompressed stream
        using var reader = new StreamReader(decompressedStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // register the inputted classmap to map column names to model attribute names
        csv.Context.RegisterClassMap<TMap>();

        // parse the records
        var records = csv.GetRecords<T>();

        // use the filter if provided
        if (filter != null)
        {
            records = records.Where(filter);
        }

        // return the records as a list
        return records.ToList();
    }

    public async Task<List<Player>> GetAllOffensivePlayersAsync()
    {
        return await LoadCsvDataAsync<Player, PlayerMap>(
            SEASON_PLAYER_STATS_URL,
            p => !string.IsNullOrWhiteSpace(p.Id) && !DEFENSIVE_POSITIONS.Contains(p.Position)
        );
    }

    public async Task<List<PlayerStatCsv>> GetAllOffensivePlayerSeasonStatsAsync()
    {
        return await LoadCsvDataAsync<PlayerStatCsv, PlayerStatCsvMap>(
            SEASON_PLAYER_STATS_URL,
            p => !string.IsNullOrWhiteSpace(p.PlayerId) && !DEFENSIVE_POSITIONS.Contains(p.Position)
        );
    }

    public async Task<List<PlayerStatCsv>> GetAllOffensivePlayerWeeklyStatsAsync()
    {
        return await LoadCsvDataAsync<PlayerStatCsv, PlayerStatCsvMap>(
            WEEKLY_PLAYER_STATS_URL,
            p => !string.IsNullOrWhiteSpace(p.PlayerId) && !DEFENSIVE_POSITIONS.Contains(p.Position)
        );
    }

    public async Task<List<PlayerInformationCsv>> GetAllPlayerInformationAsync()
    {
        return await LoadCsvDataAsync<PlayerInformationCsv, PlayerInformationCsvMap>(
            PLAYER_INFORMATION_URL,
            r => !string.IsNullOrWhiteSpace(r.Id) && r.LastSeason == CURRENT_SEASON
        );
    }

    public async Task<List<TeamSeasonStatCsv>> GetAllTeamSeasonStatsAsync()
    {
        return await LoadCsvDataAsync<TeamSeasonStatCsv, TeamSeasonStatCsvMap>(SEASON_TEAM_STATS_URL);
    }

    public async Task<List<TeamDataCsv>> GetAllTeamDataAsync()
    {
        return await LoadCsvDataAsync<TeamDataCsv, TeamDataCsvMap>(
            TEAM_DATA_URL,
            r => !string.IsNullOrWhiteSpace(r.Id) && CURRENT_TEAM_IDS_IN_NFL.Contains(r.Id)
        );
    }
}