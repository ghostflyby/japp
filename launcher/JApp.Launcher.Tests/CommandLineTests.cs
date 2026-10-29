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
}
