namespace JoseHttp;

public class JoseOptions
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

    public bool ThrowOnNotSigned { get; set; } = true;

    public SignatureFormat? AllowedSignatureFormats { get; set; }
    
    public bool ThrowOnParseError { get; set; } = true;

    public bool ThrowOnInvalid { get; set; } = true;

    public IValidator Validator { get; set; }

    public ISigner Signer { get; set; }
    
    public SignatureFormat SignatureFormat { get; set; }
}
