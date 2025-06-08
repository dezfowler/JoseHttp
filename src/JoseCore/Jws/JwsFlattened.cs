using System.Text.Json.Serialization;

namespace JoseCore.Jws;

public class JwsFlattened : IJwsContent
{
    [JsonPropertyName("protected")]
    public required string Protected { get; set; }

    [JsonPropertyName("header")]
    public IDictionary<string, object>? Header { get; set; }

    [JsonPropertyName("payload")]
    public required string Payload { get; set; }

    [JsonPropertyName("signature")]
    public required string Signature { get; set; }

    public JwsGeneral ToGeneral()
    {
        return new JwsGeneral
        {
            Payload = Payload,
            Signatures = [
                new JwsSignaturePart
                {
                     Signature = Signature,
                     Protected = Protected,
                     Header = Header,
                }
            ]
        };
    }
}
