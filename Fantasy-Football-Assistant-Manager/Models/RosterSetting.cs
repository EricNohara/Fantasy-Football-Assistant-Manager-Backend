using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;

namespace Fantasy_Football_Assistant_Manager.Models;

// Model for league roster settings (how many of each position allowed on roster)
public class RosterSetting: BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("qb_count")]
    [JsonPropertyName("qb_count")]
    public int QbCount { get; set; }

    [Column("rb_count")]
    [JsonPropertyName("rb_count")]
    public int RbCount { get; set; }

    [Column("te_count")]
    [JsonPropertyName("te_count")]
    public int TeCount { get; set; }

    [Column("wr_count")]
    [JsonPropertyName("wr_count")]
    public int WrCount { get; set; }

    [Column("k_count")]
    [JsonPropertyName("k_count")]
    public int KCount { get; set; }

    [Column("def_count")]
    [JsonPropertyName("def_count")]
    public int DefCount { get; set; }

    [Column("flex_count")]
    [JsonPropertyName("flex_count")]
    public int FlexCount { get; set; }

    [Column("bench_count")]
    [JsonPropertyName("bench_count")]
    public int BenchCount { get; set; }

    [Column("ir_count")]
    [JsonPropertyName("ir_count")]
    public int IrCount { get; set; }
}
