using CsvHelper.Configuration;
using FFOracle.Models.Supabase;

namespace FFOracle.Mappings;

public class PlayerMap: ClassMap<Player>
{
    public PlayerMap()
    {
        Map(m => m.Id).Name("player_id");
        Map(m => m.Name).Name("player_display_name");
        Map(m => m.HeadshotUrl).Name("headshot_url");
        Map(m => m.Position).Name("position");
    }
}
