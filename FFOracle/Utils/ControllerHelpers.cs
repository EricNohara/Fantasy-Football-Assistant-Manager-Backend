using FFOracle.Models;
using FFOracle.Models.Supabase;
using Supabase;

namespace FFOracle.Utils;

public class ControllerHelpers
{
    // Gets the current season and week from Supabase app settings
    public static async Task<(int Season, int Week)> GetCurrentSeasonAndWeekAsync(Client supabase)
    {
        var appStateResponse = await supabase.From<AppState>().Get();

        if (appStateResponse == null || !appStateResponse.Models.Any())
        {
            throw new Exception("App state not found");
        }

        var appState = appStateResponse.Models.First();
        return (appState.CurrentSeason, appState.CurrentWeek);
    }

    // Converts zeroes to nulls for numeric cleaning
    public static int? NullIfZero(int? value) => value == 0 ? null : value;
    public static float? NullIfZero(float? value) => value == 0f ? null : value;
    public static double? NullIfZero(double? value) => value == 0d ? null : value;
}
