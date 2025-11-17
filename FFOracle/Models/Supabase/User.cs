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
    public string? Fullname {  get; set; }

    [Column("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Column("phone_number")]
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("allow_emails")]
    [JsonPropertyName("allow_emails")]
    public bool AllowEmails { get; set; } = true;

    [Column("tokens_left")]
    [JsonPropertyName("tokens_left")]
    public int TokensLeft { get; set; } = 0;
}
