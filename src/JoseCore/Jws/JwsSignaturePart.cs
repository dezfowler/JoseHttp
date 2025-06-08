using System.Text.Json.Serialization;

namespace JoseCore.Jws;

public record JwsSignaturePart
{
    [JsonPropertyName("protected")]
    public required string Protected { get; set; }

    [JsonPropertyName("header")]
    public IDictionary<string, object>? Header { get; set; }

    [JsonPropertyName("signature")]
    public required string Signature { get; set; }
}
