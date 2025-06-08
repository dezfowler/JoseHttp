namespace JoseCore.Jws;

public interface IValidator
{
    Task<JwsValidationResult> Validate(JoseTransformContext context, IJwsContent content);
}

public interface ISigner
{
    Task<IJwsContent> Sign(JoseTransformContext context, string content, string contentType);
}
