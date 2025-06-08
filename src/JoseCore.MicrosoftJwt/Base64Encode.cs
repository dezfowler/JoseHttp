using JoseCore;
using Microsoft.IdentityModel.Tokens;

namespace JoseHttp.MicrosoftJwt;

public class Base64Encode : IBase64Url
{
    public Task<string> Decode(string encoded)
    {
        return Task.FromResult(Base64UrlEncoder.Decode(encoded));
    }

    public Task<string> Encode(string plain)
    {
        return Task.FromResult(Base64UrlEncoder.Encode(plain));
    }
}
