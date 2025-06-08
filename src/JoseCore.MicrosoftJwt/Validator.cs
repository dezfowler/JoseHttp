namespace JoseHttp.MicrosoftJwt;

using System.Threading.Tasks;
using JoseCore;
using JoseCore.Jws;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public class Validator : IValidator
{
    public required IssuerSigningKeyResolver KeyResolver { get; set; }

    public async Task<JwsValidationResult> Validate(JoseTransformContext context, IJwsContent content)
    {
        var handler = new JsonWebTokenHandler();
        var prms = new TokenValidationParameters
        {
            IssuerSigningKeyResolver = KeyResolver,
            ValidateActor = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            ValidateTokenReplay = false,
            ValidateWithLKG = false,
        };
        var compactTokens = content.ToCompact();
        foreach(var compactToken in compactTokens)
        {
            var token = new JsonWebToken($"{compactToken.Protected}.{compactToken.Payload}.{compactToken.Signature}");
            var result = await handler.ValidateTokenAsync(token, prms);
            
            if (result.IsValid) 
            {
                return new JwsValidationResult
                {
                    IsValid = result.IsValid,
                    DecodedPayload = await context.Services.Base64Url.Decode(token.EncodedPayload),
                    PayloadContentType = token.TryGetHeaderValue("cty", out string? cty) ? cty : null,
                };
            }
        }

        return new JwsValidationResult
        {
            IsValid = false
        };
    }
}
