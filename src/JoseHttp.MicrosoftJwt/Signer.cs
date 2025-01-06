namespace JoseHttp.MicrosoftJwt;

using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public class Signer : ISigner
{
    public required SigningCredentials SignCreds { get; set; }

    public async Task<IJwsContent> Sign(JoseTransformContext context, string content, string contentType)
    {
        var handler = new JsonWebTokenHandler();
        var tokenString = handler.CreateToken(content, SignCreds, new Dictionary<string, object>{ ["cty"] = contentType });
        var token = new JsonWebToken(tokenString);
        return new JwsCompact
        {
            Payload = token.EncodedPayload,
            Protected = token.EncodedHeader,
            Signature = token.EncodedSignature,
        };
    }
}