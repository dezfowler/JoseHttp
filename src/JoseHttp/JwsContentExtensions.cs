namespace JoseHttp;

public static class JwsContentExtensions
{
    public static JwsDetached ToDetached(this IJwsContent content)
    {
        var general = content.ToGeneral();
        return new JwsDetached
        {
            Payload = general.Payload,
            Signatures = general.Signatures.Select(gSig => 
            {
                return new JwsDetachedSig
                {
                    Protected = gSig.Protected,
                    Signature = gSig.Signature,
                };
            }).ToArray()
        };
    }

    public static IEnumerable<JwsCompact> ToCompact(this IJwsContent content)
    {
        var general = content.ToGeneral();
        foreach(var gSig in general.Signatures)
        {
            yield return new JwsCompact
            {
                Payload = general.Payload,
                Protected = gSig.Protected,
                Signature = gSig.Signature,
            };
        }
    }

    public static IEnumerable<JwsFlattened> ToFlattened(this IJwsContent content)
    {
        var general = content.ToGeneral();
        foreach(var gSig in general.Signatures)
        {
            yield return new JwsFlattened
            {
                Payload = general.Payload,
                Protected = gSig.Protected,
                Signature = gSig.Signature,
            };
        }
    }
}
