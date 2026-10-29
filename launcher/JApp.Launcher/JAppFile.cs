namespace JApp.Launcher;

/// <summary>
///     The japp file container (specification.md): a data pool followed by the
///     boot and launcher metadata, closed by a FileEnd trailer that repeats the
///     header magic. Reading logic lands here (and in neighbouring types) once
///     the implementation patterns are settled.
/// </summary>
public static class JAppFile
{
    /// <summary>0x5050414a ("JAPP") — the magic at both ends of a japp file.</summary>
    public const uint MagicNumber = 0x5050414a;
}
