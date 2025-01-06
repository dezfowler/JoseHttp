namespace JoseHttp;

public interface ISigning
{
    Task<IJwsContent> Sign(string content);

    Task<bool> Verify(IJwsContent jws);
}
