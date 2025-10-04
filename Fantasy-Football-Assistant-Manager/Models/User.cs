using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models;

[Table("users")]
public class User: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("team_name")]
    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }

    [Column("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
