namespace JApp.Launcher.Tests;

public static class CommandLineTests
{
    [Fact]
    public static void NoArgumentsPrintsHelpAndSucceeds()
    {
        Assert.Equal(0, CommandLine.Run([]));
    }

    [Fact]
    public static void UnknownCommandIsAUsageError()
    {
        Assert.Equal(2, CommandLine.Run(["bogus"]));
    }

    [Fact]
    public static void RunWithoutFileIsAUsageError()
    {
        Assert.Equal(2, CommandLine.Run(["run"]));
        Assert.Equal(2, CommandLine.Run(["run", "-Xmx1g"]));
    }
}
