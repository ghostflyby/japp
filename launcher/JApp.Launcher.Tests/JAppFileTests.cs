namespace JApp.Launcher.Tests;

public static class JAppFileTests
{
    [Fact]
    public static void MagicNumberSpellsJappInLittleEndian()
    {
        Assert.Equal("JAPP"u8.ToArray(), BitConverter.GetBytes(JAppFile.MagicNumber));
    }
}
