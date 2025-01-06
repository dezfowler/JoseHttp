using System.Text.Json;

namespace JoseHttp;

public class JwsTransform : JoseTransformBase
{
    internal override async Task Decode(JoseTransformContext joseTransformContext)
    {
        var step = joseTransformContext.Steps.Peek();

        var transformParams = new JwsDecodeParams
        {
            IsSigned = false,
            IsValidated = false,
        };

        step.TransformParams = transformParams;

        if (!(step.ContentType == "application/jose+json" || step.ContentType == "application/jose" || step.DetachedSignatures is { Length: >0 }))
        {
            if (joseTransformContext.Options.ThrowOnNotSigned)
            {
                throw new Exception("Content not signed");
            }
            
            return;
        }

        var jwsContent = await ParseJwsContent(joseTransformContext, step.Content, step.DetachedSignatures);
        
        transformParams.JwsContent = jwsContent;

        var result = await joseTransformContext.Options.Validator.Validate(joseTransformContext, jwsContent);

        transformParams.ValidationResult = result;

        if (joseTransformContext.Options.ThrowOnInvalid && !result.IsValid) throw new Exception("Invalid signature");

        // TODO Check this... may have unsecured coming through as signed and valid
        transformParams.IsSigned = true;
        transformParams.IsValidated = result.IsValid;

        joseTransformContext.Steps.Push(
            new JoseTransformStep
            {
                ContentType = result.PayloadContentType,
                Content = result.DecodedPayload
            }
        );
    }

    internal override async Task Encode(JoseTransformContext joseTransformContext)
    {
        var step = joseTransformContext.Steps.Peek();

        var jwsContent = await joseTransformContext.Options.Signer.Sign(joseTransformContext, step.Content, step.ContentType);

        SignatureFormat format = joseTransformContext.Options.SignatureFormat;

        IJwsContent mappedContent = format switch 
        {
            SignatureFormat.Compact => jwsContent.ToCompact().Single(),
            SignatureFormat.FlattenedJson => jwsContent.ToFlattened().Single(),
            SignatureFormat.GeneralJson => jwsContent.ToGeneral(),
            SignatureFormat.Detached => jwsContent.ToDetached(),
            _ => throw new NotSupportedException("Unknown signature format"),
        };

        JoseTransformStep mappedStep = mappedContent switch 
        {
            JwsDetached detached => new JoseTransformStep 
            {
                Content = step.Content,
                ContentType = step.ContentType,
                DetachedSignatures = detached.Signatures.Select(ds => $"{ds.Protected}..{ds.Signature}").ToArray()
            },
            JwsCompact compact => new JoseTransformStep
            {
                Content = $"{compact.Protected}.{compact.Payload}.{compact.Signature}",
                ContentType = "application/jose",
            },
            JwsFlattened flattened => new JoseTransformStep
            {
                Content = JsonSerializer.Serialize(flattened),
                ContentType = "application/jose+json"
            },
            JwsGeneral general => new JoseTransformStep
            {
                Content = JsonSerializer.Serialize(general),
                ContentType = "application/jose+json"
            },
            _ => throw new NotSupportedException("Unsupported JWS content"),
        };

        var transformParams = new JwsEncodeParams
        {
            JwsContent = jwsContent,
        };

        mappedStep.TransformParams = transformParams;

        joseTransformContext.Steps.Push(mappedStep);
    }

    private static async Task<IJwsContent?> ParseJwsContent(JoseTransformContext context, string content, string[]? sigs)
    {
        if (sigs is { Length: >0 })
        {
            // detached
            var encodedPayload = await context.Services.Base64Url.Encode(content);

            var dSigs = sigs.Select(sig => {
                var parts = sig.Split('.');
                if (!(parts is { Length: 3 })){
                    if (context.Options.ThrowOnParseError) throw new Exception("Compact JWS did not have three parts");
                    return null;
                }
                return new JwsDetachedSig 
                {
                    Protected = parts[0],
                    Signature = parts[2]
                };
            }).ToArray();

            return new JwsDetached
            {
                Payload = encodedPayload,
                Signatures = dSigs,
            };
        }

        var firstNonWhitespace = content
            .SkipWhile(char.IsWhiteSpace)
            .FirstOrDefault();

        if (firstNonWhitespace != '{')
        {
            // Compact
            var parts = content.Split('.');
            if (!(parts is { Length: 3 })){
                if (context.Options.ThrowOnParseError) throw new Exception("Compact JWS did not have three parts");
                return null;
            }

            return new JwsCompact
            {
                Protected = parts[0],
                Payload = parts[1],
                Signature = parts[2]
            };
        }

        // JSON
        using var document = JsonDocument.Parse(content);

        if (document.RootElement.TryGetProperty("signatures", out var _))
        {
            return document.Deserialize<JwsGeneral>();
        }

        return document.Deserialize<JwsFlattened>();
    }

    public class JwsDecodeParams : ITransformParams
    {
        public bool IsSigned { get; set; }
        public bool IsValidated { get; set; }
        public IJwsContent? JwsContent { get; set; }
        public JwsValidationResult ValidationResult { get; set; }
    }

    public class JwsEncodeParams : ITransformParams
    {
        public IJwsContent? JwsContent { get; set; }
    }
}
