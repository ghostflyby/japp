namespace JApp.Launcher.Tests;

public static class JavaCommandTests
{
    [Fact]
    public static void MissingCommandReturns127()
    {
        if (OperatingSystem.IsWindows())
            return; // the exec-replacement path is Unix-only

        Assert.Equal(127, JavaCommand.Run(["/nonexistent/java-binary"]));
    }
}
