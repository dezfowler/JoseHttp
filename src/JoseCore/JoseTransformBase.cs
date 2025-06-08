namespace JoseCore;

public abstract class JoseTransformBase : JoseTransform 
{
    public override Task Transform(JoseTransformContext joseTransformContext)
    {
        var step = joseTransformContext.Steps.Peek();
        step.AppliedTransform = this;
        
        return joseTransformContext.Mode switch 
        {
            TransformMode.Decode => Decode(joseTransformContext),
            TransformMode.Encode => Encode(joseTransformContext),
            _ => throw new NotImplementedException(),
        };
    }

    internal abstract Task Encode(JoseTransformContext joseTransformContext);

    internal abstract Task Decode(JoseTransformContext joseTransformContext);
}
