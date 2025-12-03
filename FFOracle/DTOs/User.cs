using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

//information on a single user
public class User
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("fullname")]
    public string Fullname { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("tokens_left")]
    public int TokensLeft { get; set; } = 0;
}
