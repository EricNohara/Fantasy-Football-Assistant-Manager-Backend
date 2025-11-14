using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace FFOracle.Models.Supabase;

[Table("users")]
public class User: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("fullname")]
    [JsonPropertyName("fullname")]
    public string Fullname {  get; set; } = string.Empty;

    [Column("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Column("tokens_left")]
    [JsonPropertyName("tokens_left")]
    public int TokensLeft { get; set; } = 0;
}
