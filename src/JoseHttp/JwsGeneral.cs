using System.Text.Json.Serialization;

namespace JoseHttp;

public class JwsGeneral : IJwsContent
{
    [JsonPropertyName("payload")]
    public required string Payload { get; set; }

    [JsonPropertyName("signatures")]
    public required JwsSignaturePart[] Signatures { get; set; }

    public JwsGeneral ToGeneral()
    {
        return this;
    }


}
