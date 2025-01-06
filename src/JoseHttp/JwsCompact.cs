namespace JoseHttp;

public class JwsCompact : IJwsContent
{
    public required string Protected { get; set; }

    public required string Payload { get; set; }

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
                }
            ]
        };
    }
}
