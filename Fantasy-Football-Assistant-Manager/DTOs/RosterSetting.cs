using System;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.DTOs;

public class RosterSetting
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("qb_count")]
    public int QbCount { get; set; }

    [JsonPropertyName("rb_count")]
    public int RbCount { get; set; }

    [JsonPropertyName("te_count")]
    public int TeCount { get; set; }

    [JsonPropertyName("wr_count")]
    public int WrCount { get; set; }

    [JsonPropertyName("k_count")]
    public int KCount { get; set; }

    [JsonPropertyName("def_count")]
    public int DefCount { get; set; }

    [JsonPropertyName("flex_count")]
    public int FlexCount { get; set; }

    [JsonPropertyName("bench_count")]
    public int BenchCount { get; set; }

    [JsonPropertyName("ir_count")]
    public int IrCount { get; set; }
}
