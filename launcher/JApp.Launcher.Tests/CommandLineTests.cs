namespace JApp.Launcher.Tests;

public static class CommandLineTests
{
    [Fact]
    public static void NoArgumentsPrintsHelpAndExitsZero()
    {
        Assert.Equal(0, CommandLine.Run([]));
    }

    [Theory]
    [InlineData("help")]
    [InlineData("-help")]
    [InlineData("--help")]
    public static void HelpVariantsExitZero(string mode)
    {
        Assert.Equal(0, CommandLine.Run([mode]));
    }

    [Theory]
    [InlineData("version")]
    [InlineData("--version")]
    public static void VersionVariantsExitZero(string mode)
    {
        Assert.Equal(0, CommandLine.Run([mode]));
    }

    [Fact]
    public static void UnknownModeExitsOne()
    {
        Assert.Equal(1, CommandLine.Run(["bogus"]));
    }

    [Fact]
    public static void RunWithoutFileExitsOne()
    {
        Assert.Equal(1, CommandLine.Run(["run"]));
    }

    [Fact]
    public static void RunHelpExitsZero()
    {
        Assert.Equal(0, CommandLine.Run(["run", "--help"]));
    }

    [Fact]
    public static void RunRejectsUnknownOptionWithOne()
    {
        Assert.Equal(1, CommandLine.Run(["run", "-Xmx1g"]));
    }

    [Fact]
    public static void RunMissingJappFileSurfacesError()
    {
        // The file does not exist: metadata reading fails, message goes to stderr, exit 1.
        Assert.Equal(1, CommandLine.Run(["run", "/nonexistent/app.japp"]));
    }

    // ---- dual launch mode (embedded archive), exercised through the injected run seam ----

    private const int FakeExitCode = 7;

    private static (string File, IReadOnlyList<string> JvmOptions, IReadOnlyList<string> AppArgs)? _invocation;

    private static int CaptureRun(string file, IReadOnlyList<string> jvmOptions, IReadOnlyList<string> appArgs)
    {
        _invocation = (file, jvmOptions, appArgs);
        return FakeExitCode;
    }

    private static void AssertCaptured(string file, string[] jvmOptions, string[] appArgs)
    {
        Assert.NotNull(_invocation);
        Assert.Equal(Path.GetFullPath(file), _invocation.Value.File);
        Assert.Equal(jvmOptions, _invocation.Value.JvmOptions);
        Assert.Equal(appArgs, _invocation.Value.AppArgs);
    }

    [Fact]
    public static void EmbeddedModeForwardsEveryArgumentVerbatim()
    {
        _invocation = null;
        Assert.Equal(FakeExitCode,
            CommandLine.Run(["-J-Xmx1g", "--", "help", "run"], "/tmp/embedded.japp", CaptureRun));
        AssertCaptured("/tmp/embedded.japp", [], ["-J-Xmx1g", "--", "help", "run"]);

        _invocation = null;
        Assert.Equal(FakeExitCode, CommandLine.Run([], "/tmp/embedded.japp", CaptureRun));
        AssertCaptured("/tmp/embedded.japp", [], []);
    }

    [Fact]
    public static void EmbeddedGatewayParsesJvmOptionsAndSeparator()
    {
        _invocation = null;
        Assert.Equal(FakeExitCode,
            CommandLine.Run(["--japp", "run", "-J-Xmx4g", "-J-XX:+UseZGC", "--", "a", "b"],
                "/tmp/embedded.japp", CaptureRun));
        AssertCaptured("/tmp/embedded.japp", ["-Xmx4g", "-XX:+UseZGC"], ["a", "b"]);
    }

    [Fact]
    public static void EmbeddedGatewayTreatsNonFilePositionalsAsAppArguments()
    {
        _invocation = null;
        Assert.Equal(FakeExitCode,
            CommandLine.Run(["--japp", "run", "not-a-file", "arg"], "/tmp/embedded.japp", CaptureRun));
        AssertCaptured("/tmp/embedded.japp", [], ["not-a-file", "arg"]);
    }

    [Fact]
    public static void EmbeddedGatewayUsesExistingPositionalAsExplicitFile()
    {
        var existing = Path.GetTempFileName();

        _invocation = null;
        Assert.Equal(FakeExitCode,
            CommandLine.Run(["--japp", "run", existing, "x"], "/tmp/embedded.japp", CaptureRun));
        AssertCaptured(existing, [], ["x"]);
    }

    [Fact]
    public static void EmbeddedGatewayHelpVariantsExitZero()
    {
        Assert.Equal(0, CommandLine.Run(["--japp"], "/tmp/embedded.japp", CaptureRun));
        Assert.Equal(0, CommandLine.Run(["--japp", "help"], "/tmp/embedded.japp", CaptureRun));
        Assert.Equal(0, CommandLine.Run(["--japp", "run", "--help"], "/tmp/embedded.japp", CaptureRun));
    }

    [Fact]
    public static void EmbeddedGatewayUnknownModeExitsOne()
    {
        Assert.Equal(1, CommandLine.Run(["--japp", "bogus"], "/tmp/embedded.japp", CaptureRun));
    }

    [Fact]
    public static void BareModeKeepsJavaCompatibleParsing()
    {
        _invocation = null;
        Assert.Equal(FakeExitCode,
            CommandLine.Run(["run", "-J-Xmx1g", "some.japp", "--", "-J-x"], null, CaptureRun));
        AssertCaptured("some.japp", ["-Xmx1g"], ["-J-x"]);

        _invocation = null;
        Assert.Equal(1, CommandLine.Run(["run", "-Xmx1g"], null, CaptureRun));
        Assert.Null(_invocation);
    }
}
