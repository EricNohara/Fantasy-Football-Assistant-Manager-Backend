using System;
using System.Text.Json.Serialization;

namespace FFOracle.DTOs;

public class TeamOffensiveStat
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("completions")]
    public int? Completions { get; set; }

    [JsonPropertyName("attempts")]
    public int? Attempts { get; set; }

    [JsonPropertyName("passing_yards")]
    public int? PassingYards { get; set; }

    [JsonPropertyName("passing_tds")]
    public int? PassingTds { get; set; }

    [JsonPropertyName("passing_interceptions")]
    public int? PassingInterceptions { get; set; }

    [JsonPropertyName("sacks_against")]
    public int? SacksAgainst { get; set; }

    [JsonPropertyName("fumbles_against")]
    public int? FumblesAgainst { get; set; }

    [JsonPropertyName("carries")]
    public int? Carries { get; set; }

    [JsonPropertyName("rushing_yards")]
    public int? RushingYards { get; set; }

    [JsonPropertyName("rushing_tds")]
    public int? RushingTds { get; set; }

    [JsonPropertyName("receptions")]
    public int? Receptions { get; set; }

    [JsonPropertyName("targets")]
    public int? Targets { get; set; }

    [JsonPropertyName("receiving_yards")]
    public int? ReceivingYards { get; set; }

    [JsonPropertyName("receiving_tds")]
    public int? ReceivingTds { get; set; }
}
