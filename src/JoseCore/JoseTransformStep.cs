namespace JoseCore;

public class JoseTransformStep
{
    public string? ContentType { get; set; }

    public string[]? DetachedSignatures { get; set; }

    public required string Content { get; set; }

    public JoseTransform? AppliedTransform { get; set; }

    public ITransformParams TransformParams { get; set; }

    // ...
}
