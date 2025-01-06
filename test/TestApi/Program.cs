using System.Buffers;
using System.Net.Http.Headers;
using System.Net.Mime;
using JoseHttp;
using JoseHttp.Client;
using JoseHttp.MicrosoftJwt;
using JoseHttp.Server;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

IDictionary<string, JsonWebKey> TestingKeys = new Dictionary<string, JsonWebKey>{
    ["A"] = JsonWebKey.Create(/* language=JSON */"""
{ 
  "kty": "oct",
  "k": "AyM1SysPpbyDfgZld3umj1qzKObwVMkoqQ-EstJQLr_T-1qS0gZH75aKtMN3Yj0iPS4hcgUuTwjAzZr1Z9CAow"
}   
"""),
    ["B"] = JsonWebKey.Create(/* language=JSON */"""
{
  "kty":"RSA",
  "n":"ofgWCuLjybRlzo0tZWJjNiuSfb4p4fAkd_wWJcyQoTbji9k0l8W26mPddxHmfHQp-Vaw-4qPCJrcS2mJPMEzP1Pt0Bm4d4QlL-yRT-SFd2lZS-pCgNMsD1W_YpRPEwOWvG6b32690r2jZ47soMZo9wGzjb_7OMg0LOL-bSf63kpaSHSXndS5z5rexMdbBYUsLA9e-KXBdQOS-UTo7WTBEMa2R2CapHg665xsmtdVMTBQY4uDZlxvb3qCo5ZwKh9kG4LT6_I5IhlJH7aGhyxXFvUK-DWNmoudF8NAco9_h9iaGNj8q2ethFkMLs91kzk2PAcDTW9gb54h4FRWyuXpoQ",
  "e":"AQAB",
  "d":"Eq5xpGnNCivDflJsRQBXHx1hdR1k6Ulwe2JZD50LpXyWPEAeP88vLNO97IjlA7_GQ5sLKMgvfTeXZx9SE-7YwVol2NXOoAJe46sui395IW_GO-pWJ1O0BkTGoVEn2bKVRUCgu-GjBVaYLU6f3l9kJfFNS3E0QbVdxzubSu3Mkqzjkn439X0M_V51gfpRLI9JYanrC4D4qAdGcopV_0ZHHzQlBjudU2QvXt4ehNYTCBr6XCLQUShb1juUO1ZdiYoFaFQT5Tw8bGUl_x_jTj3ccPDVZFD9pIuhLhBOneufuBiB4cS98l2SR_RQyGWSeWjnczT0QU91p1DhOVRuOopznQ",
  "p":"4BzEEOtIpmVdVEZNCqS7baC4crd0pqnRH_5IB3jw3bcxGn6QLvnEtfdUdiYrqBdss1l58BQ3KhooKeQTa9AB0Hw_Py5PJdTJNPY8cQn7ouZ2KKDcmnPGBY5t7yLc1QlQ5xHdwW1VhvKn-nXqhJTBgIPgtldC-KDV5z-y2XDwGUc",
  "q":"uQPEfgmVtjL0Uyyx88GZFF1fOunH3-7cepKmtH4pxhtCoHqpWmT8YAmZxaewHgHAjLYsp1ZSe7zFYHj7C6ul7TjeLQeZD_YwD66t62wDmpe_HlB-TnBA-njbglfIsRLtXlnDzQkv5dTltRJ11BKBBypeeF6689rjcJIDEz9RWdc",
  "dp":"BwKfV3Akq5_MFZDFZCnW-wzl-CCo83WoZvnLQwCTeDv8uzluRSnm71I3QCLdhrqE2e9YkxvuxdBfpT_PI7Yz-FOKnu1R6HsJeDCjn12Sk3vmAktV2zb34MCdy7cpdTh_YVr7tss2u6vneTwrA86rZtu5Mbr1C1XsmvkxHQAdYo0",
  "dq":"h_96-mK1R_7glhsum81dZxjTnYynPbZpHziZjeeHcXYsXaaMwkOlODsWa7I9xXDoRwbKgB719rrmI2oKr6N3Do9U0ajaHF-NKJnwgjMd2w9cjz3_-kyNlxAr2v4IKhGNpmM5iIgOS1VZnOZ68m6_pbLBSp3nssTdlqvd0tIiTHU",
  "qi":"IYd7DHOhrWvxkwPQsRM2tOgrjbcrfvtQJipd-DlcxyVuuM9sQLdgjVk2oy26F0EmpScGLq2MowX7fhd_QJQ3ydy5cY7YIBi87w93IKLEdfnbJtoOPLUW0ITrJReOgo1cq9SbsxYawBgfp_gh6A5603k2-ZQwVK0JKSHuLFkuQ3U"
}   
"""),
    ["C"] = JsonWebKey.Create(/* language=JSON */"""
{
  "kty":"EC",
  "crv":"P-256",
  "x":"f83OJ3D2xF1Bg8vub9tLe1gHMzV76e8Tus9uPHvRVEU",
  "y":"x_FEzRu9m36HLN_tue659LNpXW6pCyStikYjKIWI5a0",
  "d":"jpsQnnGQmL-YBIffH1136cspYG6-0iY7X1fCE9-E9LI"
}
"""),
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IBase64Url, Base64Encode>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Map("/sig", HandleSigned);

app.Map("/enc", HandleEncrypted);

app.Map("/sigenc", HandleSignedAndEncrypted);

app.Map("/auto", HandleAuto);

app.Map("/client/sig", HandleClientSigned);


// app.Run(async context =>
// {
//     context.Response.StatusCode = 404;
//     await context.Response.WriteAsync("Hello from non-Map delegate.");
// });

app.Run();


void HandleSigned(IApplicationBuilder app)
{
    var logger = app.ApplicationServices.GetRequiredService<ILogger<String>>();
    app.UseMiddleware<JoseMiddleware>(new JoseOptions
    {
        RequestTransform = new JwsTransform(),
        ResponseTransform = JoseTransform.None,
        Validator = new Validator
        {
            KeyResolver = KeyResolver
        }
    });

    app.Run(async context =>
    {
        //echo the request back out
        logger.LogInformation("In the request handler: {length}", context.Request.ContentLength);

        if (context.Items["JoseRequestContext"] is JoseTransformContext joseContext)
        {
            var jwsParams = joseContext.Steps.Select(t => t.TransformParams).OfType<JwsTransform.JwsDecodeParams>().FirstOrDefault();
            if (jwsParams is {})
            {
                logger.LogInformation("JWS step found: {IsSigned} {IsValidated} {SigType}", jwsParams.IsSigned, jwsParams.IsValidated, jwsParams.JwsContent.GetType().Name);                
            }
            else
            {
                logger.LogInformation("No JWS step in context");
            }
        }
        context.Response.ContentLength = context.Request.ContentLength;
        context.Request.Body.Position = 0;
        await context.Request.Body.CopyToAsync(context.Response.Body);
        context.Response.Body.Position = 0;
    });
}

void HandleEncrypted(IApplicationBuilder app)
{
    app.UseMiddleware<JoseMiddleware>(new JoseOptions
    {
        RequestTransform = new JweTransform
        {
            
        },
        ResponseTransform = new JweTransform
        {
            
        },
    });

    app.Run(async context =>
    {
        //echo the request back out
        await context.Request.Body.CopyToAsync(context.Response.Body);
    });
}

void HandleSignedAndEncrypted(IApplicationBuilder app)
{
    app.UseMiddleware<JoseMiddleware>(new JoseOptions
    {
        RequestTransform = JoseTransform.CreateChain(
            new JweTransform{}, 
            new JwsTransform
            {
            
            }),
        ResponseTransform = JoseTransform.CreateChain(
            new JwsTransform
            {
                
            },
            new JweTransform{})
    });

    app.Run(async context =>
    {
        //echo the request back out
        await context.Request.Body.CopyToAsync(context.Response.Body);
    });
}

void HandleAuto(IApplicationBuilder app)
{
    app.UseMiddleware<JoseMiddleware>(new JoseOptions
    {
        RequestTransform = JoseTransform.Detect,
        ResponseTransform = JoseTransform.None,
    });

    app.Run(async context =>
    {
        //echo the request back out
        await context.Request.Body.CopyToAsync(context.Response.Body);
    });
}

void HandleClientSigned(IApplicationBuilder app)
{
    var logger = app.ApplicationServices.GetRequiredService<ILogger<String>>();
    var base64Url = app.ApplicationServices.GetRequiredService<IBase64Url>();

    app.Run(async context  =>
    {
        //echo the request back out
        logger.LogInformation("In the request handler: {length}", context.Request.ContentLength);

        var req = await context.Request.ReadFromJsonAsync<ClientRequest>();

        var client = new HttpClient(
            new JoseHandler(
                new JoseOptions
                {
                    RequestTransform = new JwsTransform(),
                    ResponseTransform = JoseTransform.None,
                    Signer = new Signer
                    {
                        SignCreds = new SigningCredentials(JsonWebKey.Create(req.signingKey), req.algo)
                    },
                    SignatureFormat = SignatureFormat.Compact
                },
                new JoseServices 
                {
                    Base64Url = base64Url
                } 
            )
        );

        var response = await client.PostAsync(req.url, new StringContent(req.payload, new MediaTypeHeaderValue(MediaTypeNames.Application.Json)));

        if (response.RequestMessage.Options.GetValueOrDefault("JoseRequestContext") is JoseTransformContext joseContext)
        {
            var jwsParams = joseContext.Steps.Select(t => t.TransformParams).OfType<JwsTransform.JwsEncodeParams>().FirstOrDefault();
            if (jwsParams is {})
            {
                logger.LogInformation("JWS step found: {SigType}", jwsParams.JwsContent.GetType().Name);                
            }
            else
            {
                logger.LogInformation("No JWS step in context");
            }
        }
        
        await response.Content.CopyToAsync(context.Response.Body);
    });
}


IEnumerable<SecurityKey> KeyResolver(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
{

    if (securityToken.SigningKey != null) yield return securityToken.SigningKey;

    // Embedded key
    if (securityToken is JsonWebToken jsonToken && jsonToken.TryGetHeaderValue<string>("jwk", out var signingKeyJson))
    {
        var jwk = JsonWebKey.Create(signingKeyJson);
        yield return jwk;
    }

    // TODO Resolve using Kid or from remote JWKS

    if (TestingKeys.TryGetValue(kid, out var key))
    {
        yield return key;
    }

    foreach(var testKey in TestingKeys.Values)
    {
        yield return testKey;
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


record ClientRequest(string url, string payload, string signingKey, string algo);
