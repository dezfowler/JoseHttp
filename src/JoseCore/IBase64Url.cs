namespace JoseCore;

public interface IBase64Url
{
    Task<string> Encode(string plain);

    Task<string> Decode(string encoded);
}
