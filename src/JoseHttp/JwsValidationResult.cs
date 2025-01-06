namespace JoseHttp;

public class JwsValidationResult
{
    public bool IsValid { get; set; }

    public string? PayloadContentType { get; set; } 

    public string? DecodedPayload { get; set; }
}