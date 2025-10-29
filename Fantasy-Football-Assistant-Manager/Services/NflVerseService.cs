using System.Globalization;
using CsvHelper;
using Fantasy_Football_Assistant_Manager.Models;
using Fantasy_Football_Assistant_Manager.Mappings;

namespace Fantasy_Football_Assistant_Manager.Services;

public class NflVerseService
{
    private readonly HttpClient _httpClient;
    // url for updated NFL player stats maintained by nflverse
    private const string SEASONAL_PLAYER_STATS_URL = "https://github.com/nflverse/nflverse-data/releases/download/stats_player/stats_player_reg_2025.csv";

    // used for fast loopups for defensive positions (we only store offensive players)
    private HashSet<string> DEFENSIVE_POSITIONS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "DL", "DE", "DT", "NT", "LB", "ILB", "MLB", "OLB", "DB", "CB", "S", "FS", "SS"
    };

    public NflVerseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // function to get all offensive players as Player type
    public async Task<List<Player>> GetAllOffensivePlayersAsync()
    {
        // fetch all player stats from nflverse CSV
        using var res = await _httpClient.GetAsync(SEASONAL_PLAYER_STATS_URL);
        res.EnsureSuccessStatusCode();

        // read the contents of the CSV as a stream to parse the CSV
        using var stream = await res.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // register the player stat mappings from the csv format to our column names
        csv.Context.RegisterClassMap<PlayerMap>();

        // read all records from the CSV and convert it to a list of player stats
        var records = csv.GetRecords<Player>().ToList();

        // Filter out defensive players
        var offensivePlayers = records
            .Where(p => !string.IsNullOrWhiteSpace(p.Id) && !DEFENSIVE_POSITIONS.Contains(p.Position))
            .ToList();

        return offensivePlayers;
    }

    // function to get all offensive season stats with the associated player id
    public async Task<List<PlayerStatCsv>> GetAllOffensiveSeasonStatsAsync()
    {
        // fetch all player stats from nflverse CSV
        using var res = await _httpClient.GetAsync(SEASONAL_PLAYER_STATS_URL);
        res.EnsureSuccessStatusCode();

        // read the contents of the CSV as a stream to parse the CSV
        using var stream = await res.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // register the player stat mappings from the csv format to our column names
        csv.Context.RegisterClassMap<PlayerStatCsvMap>();

        // read all records from the CSV and convert it to a list of player stats
        var records = csv.GetRecords<PlayerStatCsv>().ToList();

        // Filter out defensive players
        var offensivePlayers = records
            .Where(p => !string.IsNullOrWhiteSpace(p.PlayerId) && !DEFENSIVE_POSITIONS.Contains(p.Position))
            .ToList();

        return offensivePlayers;
    }
}