using System.Globalization;
using CsvHelper;
using Fantasy_Football_Assistant_Manager.Models;
using Fantasy_Football_Assistant_Manager.Mappings;

namespace Fantasy_Football_Assistant_Manager.Services;

public class NflVerseService
{
    private readonly HttpClient _httpClient;
    // url for updated NFL player stats maintained by nflverse
    private const string PLAYER_STATS_URL = "https://github.com/nflverse/nflverse-data/releases/download/stats_player/stats_player_reg_2025.csv";

    public NflVerseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PlayerStat>> GetAllPlayerStatsAsync()
    {
        // fetch all player stats from nflverse CSV file
        using var res = await _httpClient.GetAsync(PLAYER_STATS_URL);
        res.EnsureSuccessStatusCode();

        // read the contents of the CSV as a stream to parse the CSV
        using var stream = await res.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // register the player stat mappings from the csv format to our column names
        csv.Context.RegisterClassMap<PlayerStatMap>();

        // read all records from the CSV and convert it to a list of player stats
        var records = csv.GetRecords<PlayerStat>().ToList();

        // generate GUIDs for each record (needed for supabase model when we want to insert into our database later on)
        foreach(var record in records)
        {
            record.Id = Guid.NewGuid();
        }

        return records;
    }
}
