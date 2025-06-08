namespace JoseCore.Jws;

[Flags]
public enum SignatureFormat
{
    Compact = 1,
    FlattenedJson = 2,
    GeneralJson = 4,
    Detached = 8,
}
