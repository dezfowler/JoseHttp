namespace JoseCore.Jws;

public class JwsDetached : IJwsContent
{
    public required string Payload { get; set; }

    public required JwsDetachedSig[] Signatures { get; set; }

    public JwsGeneral ToGeneral()
    {
        return new JwsGeneral
        {
            Payload = Payload,
            Signatures = Signatures.Select(dSig => 
                new JwsSignaturePart
                {
                     Signature = dSig.Signature,
                     Protected = dSig.Protected,
                }
            ).ToArray()
        };
    }
}
