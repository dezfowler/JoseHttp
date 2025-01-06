namespace JoseHttp;

public class JoseTransform
{
    public static readonly JoseTransform Detect = new DetectTransform();

    public static readonly JoseTransform None = new NoopTransform();

    private JoseTransform(JoseTransform[] joseTransforms)
    {

    }

    protected JoseTransform() { }

    public static JoseTransform CreateChain(params JoseTransform[] joseTransforms)
    {
        return new JoseTransform(joseTransforms);
    }

    internal virtual Task Transform(JoseTransformContext joseTransformContext)
    {
        throw new NotImplementedException();
    }

    class DetectTransform : JoseTransform 
    {
        internal override Task Transform(JoseTransformContext joseTransformContext)
        {
            var step = joseTransformContext.Steps.Peek();
            step.AppliedTransform = this;
        
            throw new NotImplementedException();
        }
    }

    class NoopTransform : JoseTransform
    {
        internal override Task Transform(JoseTransformContext joseTransformContext)
        {
            var step = joseTransformContext.Steps.Peek();
            step.AppliedTransform = this;
        
            return Task.CompletedTask;
        }
    }
}
