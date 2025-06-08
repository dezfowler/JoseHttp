using JoseCore;

namespace JoseHttp;

public class JoseHttpOptions
{
    public JoseTransform RequestTransform { get; set; } = JoseTransform.None;

    public JoseTransform ResponseTransform { get; set; } = JoseTransform.None;

    /// <summary>
    /// Detached signature header names to check when decoding
    /// </summary>
    public string?[]? DetachedSignatureHeaderNames { get; set; }

    /// <summary>
    /// Detached signature header name to use when encoding
    /// </summary>
    public string? DetachedSignatureHeaderName { get; set; }
}
