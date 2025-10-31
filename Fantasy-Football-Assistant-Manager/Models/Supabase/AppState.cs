using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models.Supabase;

[Table("app_state")]
public class AppState: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("current_season")]
    [JsonPropertyName("current_season")]
    public int CurrentSeason { get; set; } = -1;

    [Column("current_week")]
    [JsonPropertyName("current_week")]
    public int CurrentWeek { get; set; } = -1;

    [Column("last_updated")]
    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
