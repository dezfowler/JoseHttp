namespace JoseCore;

public abstract class JoseTransform
{
    public static readonly JoseTransform Detect = new DetectTransform();

    public static readonly JoseTransform None = new NoopTransform();

    protected JoseTransform() { }

    public static JoseTransform CreateChain(params JoseTransform[] joseTransforms)
    {
        return new TransformChain(joseTransforms);
    }

    public abstract Task Transform(JoseTransformContext joseTransformContext);

    class TransformChain(JoseTransform[] joseTransforms) : JoseTransform
    {
        public override Task Transform(JoseTransformContext joseTransformContext)
        {        
            throw new NotImplementedException();
        }
    }

    class DetectTransform : JoseTransform 
    {
        public override Task Transform(JoseTransformContext joseTransformContext)
        {
            var step = joseTransformContext.Steps.Peek();
            step.AppliedTransform = this;
        
            throw new NotImplementedException();
        }
    }
    class NoopTransform : JoseTransform
    {
        public override Task Transform(JoseTransformContext joseTransformContext)
        {
            var step = joseTransformContext.Steps.Peek();
            step.AppliedTransform = this;
        
            return Task.CompletedTask;
        }
    }
}
