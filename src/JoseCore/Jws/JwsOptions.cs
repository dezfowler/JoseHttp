namespace JoseCore.Jws;

public class JwsOptions
{
    public SignatureFormat? AllowedSignatureFormats { get; set; }
    
    public SignatureFormat SignatureFormat { get; set; }

    public ISigner Signer { get; set; }

    public bool ThrowOnNotSigned { get; set; } = true;

    public bool ThrowOnParseError { get; set; } = true;

    public bool ThrowOnInvalid { get; set; } = true;

    public IValidator Validator { get; set; }
}
