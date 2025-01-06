using System.Buffers.Text;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JoseHttp.Server;

public class JoseMiddleware
{
    private readonly RequestDelegate next;
    private readonly JoseOptions joseOptions;

    public JoseMiddleware(RequestDelegate next, JoseOptions joseOptions)
    {
        this.next = next;
        this.joseOptions = joseOptions;
    }
    
    public async Task InvokeAsync(HttpContext context, IBase64Url base64Url, ILogger<JoseMiddleware> logger)
    {
        var contentType = context.Request.ContentType;

        context.Request.EnableBuffering();
        
        string requestContent = await ReadAllAsStringAsync(context.Request.Body);

        var joseRequestContext = new JoseTransformContext 
        {
            Mode = TransformMode.Decode,
            Options = joseOptions,
            Services = new JoseServices 
            {
                Base64Url = base64Url
            } 
        };

        joseRequestContext.Steps.Push(new JoseTransformStep
        {
            ContentType = contentType,
            Content = requestContent,
            DetachedSignatures = GetDetachedSigs(context.Request),
        });

        await joseOptions.RequestTransform.Transform(joseRequestContext);

        context.Items["JoseRequestContext"] = joseRequestContext;
        var decodedRequest = joseRequestContext.Steps.Peek();
        logger.LogInformation("Decoded request content: {decodedRequest}", decodedRequest.Content);
        
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(decodedRequest.Content));
        context.Request.ContentLength = context.Request.Body.Length;
        
        using var responseBuffer = new MemoryStream();
        var originalBodyStream = context.Response.Body;
        context.Response.Body = responseBuffer;

        await next(context);

        var responseContent = await ReadAllAsStringAsync(context.Response.Body);

        logger.LogInformation("Decoded response content: {responseContent}", responseContent);

        var joseResponseContext = new JoseTransformContext 
        {
            Mode = TransformMode.Encode,
            Options = joseOptions,
            Services = new JoseServices 
            {
                Base64Url = base64Url
            } 
        };
        joseResponseContext.Steps.Push(new JoseTransformStep
        {
            ContentType = context.Response.ContentType,
            Content = responseContent,
        });

        await joseOptions.ResponseTransform.Transform(joseResponseContext);

        context.Items["JoseResponseContext"] = joseResponseContext;
        
        var final = joseResponseContext.Steps.Peek();
        var finalResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(final.Content));
        
        context.Response.ContentType = final.ContentType;
        context.Response.ContentLength = finalResponseStream.Length;
        if (final.DetachedSignatures != null)
        {
            context.Response.Headers.TryAdd(joseOptions.DetachedSignatureHeaderName ?? "x-jws-signature", final.DetachedSignatures);
        }

        context.Response.Body = originalBodyStream;
        await finalResponseStream.CopyToAsync(context.Response.Body);

        static Task<string> ReadAllAsStringAsync(Stream stream)
        {
            using var sr = new StreamReader(stream);
            var stringValue = sr.ReadToEndAsync();
            stream.Position = 0;
            return stringValue;
        }
    }

    private string[] GetDetachedSigs(HttpRequest request)
    {
        string[] names = (joseOptions.DetachedSignatureHeaderNames ?? (joseOptions.DetachedSignatureHeaderName != null ? new string[]{ joseOptions.DetachedSignatureHeaderName } : null ) ?? [ "x-jws-signature" ])
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray()!;

        return names
            .SelectMany(k => request.Headers.TryGetValue(k, out var signatures) ? signatures.Where<string>(s => !string.IsNullOrWhiteSpace(s)).ToArray() : Array.Empty<string>())            
            .ToArray();
    }
}