namespace JoseHttp;

public class JoseTransformContext 
{
    public Stack<JoseTransformStep> Steps { get; set; } = new Stack<JoseTransformStep>();

    public TransformMode Mode { get; set; }

    public required JoseOptions Options { get; set; }

    public required JoseServices Services { get; set; }
}
