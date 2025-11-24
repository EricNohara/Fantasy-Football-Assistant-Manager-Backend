using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("player_espn_ids")]
public class PlayerEspnId: BaseModel
{
    [PrimaryKey("player_id", false)]
    [Column("player_id")]
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; }

    [PrimaryKey("espn_id", false)]
    [Column("espn_id")]
    [JsonPropertyName("espn_id")]
    public string EspnId { get; set; }
}
