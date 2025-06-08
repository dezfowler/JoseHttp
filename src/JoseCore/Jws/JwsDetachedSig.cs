namespace JoseCore.Jws;


public class JwsDetachedSig
{
    public required string Protected { get; set; }
    public required string Signature { get; set; }
}
