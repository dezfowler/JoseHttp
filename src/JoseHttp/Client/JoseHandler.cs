
using JoseCore;
using System.Net.Http.Headers;

namespace JoseHttp.Client;

public class JoseHandler : DelegatingHandler
{
    private readonly JoseServices joseServices;
    private readonly JoseHttpOptions joseOptions;

    public JoseHandler(JoseHttpOptions joseOptions, JoseServices joseServices)
        // TODO this isn't right... need to do handler proper
        : base(new HttpClientHandler())
    {
        this.joseOptions = joseOptions;
        this.joseServices = joseServices;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var joseRequestContext = new JoseTransformContext
        {
            //Options = joseOptions,
            Services = joseServices,
            Mode = TransformMode.Encode,
        };
        request.Options.TryAdd("JoseRequestContext", joseRequestContext);

        var joseResponseContext = new JoseTransformContext
        {
            //Options = joseOptions,
            Services = joseServices,
            Mode = TransformMode.Decode,
        };
        request.Options.TryAdd("JoseResponseContext", joseResponseContext);

        // Encode
        if (request.Content != null)
        {
            await request.Content.LoadIntoBufferAsync();
            var stringContent = await request.Content.ReadAsStringAsync();

            joseRequestContext.Steps.Push(
                new JoseTransformStep
                {
                    Content = stringContent,
                    ContentType = request.Content.Headers.ContentType?.MediaType,
                }
            );
            await joseOptions.RequestTransform.Transform(joseRequestContext);
            var finalRequestStep = joseRequestContext.Steps.Peek();
            
            if (finalRequestStep.DetachedSignatures != null)
            {
                request.Headers.Add(joseOptions.DetachedSignatureHeaderName ?? "x-jws-signature", finalRequestStep.DetachedSignatures);
            }

            request.Content = new StringContent(finalRequestStep.Content, finalRequestStep.ContentType!= null ? MediaTypeHeaderValue.Parse(finalRequestStep.ContentType) : null);
        }

        var response = await base.SendAsync(request, cancellationToken);

        await response.Content.LoadIntoBufferAsync();
        var responseString = await response.Content.ReadAsStringAsync();

        joseResponseContext.Steps.Push(
            new JoseTransformStep
            {
                Content = responseString,
                ContentType = response.Content.Headers.ContentType?.MediaType,
                DetachedSignatures = GetDetachedSigs(response.Headers)
            }
        );
        await joseOptions.ResponseTransform.Transform(joseResponseContext);
        var finalResponseStep = joseResponseContext.Steps.Peek();
        response.Content = new StringContent(finalResponseStep.Content, finalResponseStep.ContentType!= null ? MediaTypeHeaderValue.Parse(finalResponseStep.ContentType) : null);
        
        return response;
    }

    private string[] GetDetachedSigs(HttpResponseHeaders headers)
    {
        string[] names = (joseOptions.DetachedSignatureHeaderNames ?? (joseOptions.DetachedSignatureHeaderName != null ? new string[]{ joseOptions.DetachedSignatureHeaderName } : null ) ?? [ "x-jws-signature" ])
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray()!;

        return names
            .SelectMany(k => headers.TryGetValues(k, out var signatures) ? signatures.Where<string>(s => !string.IsNullOrWhiteSpace(s)).ToArray() : Array.Empty<string>())            
            .ToArray();
    }
}
