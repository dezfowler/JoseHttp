namespace JoseCore;

public class JoseTransformContext 
{
    /// <summary>
    /// Steps captures the raw data at each stage of the transform process
    /// </summary>
    /// <remarks>
    /// If a request is signed and encrypted then the first step in the stack 
    /// would hold the JWE content, the next will hold the decrypted JWS content
    /// and the last will hold the decoded content that was signed.
    /// </remarks>
    public Stack<JoseTransformStep> Steps { get; set; } = new Stack<JoseTransformStep>();

    public TransformMode Mode { get; set; }

    // Should options be here or part of the configured pipeline?
    //public required JoseOptions Options { get; set; }

    // Should services be here or injected?
    public required JoseServices Services { get; set; }
}
