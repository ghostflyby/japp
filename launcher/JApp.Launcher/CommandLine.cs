namespace JApp.Launcher;

/// <summary>
///     Command dispatch, mirroring the Rust launcher's surface (native/src/main.rs):
///     help, version and run. The run pipeline — japp file reading, JVM selection,
///     boot-launcher handoff — is filled in once the implementation patterns are
///     settled.
/// </summary>
internal static class CommandLine
{
    private const int ExitSuccess = 0;
    private const int ExitFailure = 1;
    private const int ExitUsage = 2;

    internal static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            WriteHelp(Console.Error);
            return ExitSuccess;
        }

        return args[0] switch
        {
            "help" or "-h" or "--help" or "-?" => HandleHelp(),
            "version" or "-v" or "--version" => HandleVersion(),
            "run" => HandleRun(args[1..]),
            _ => UnknownCommand(args[0]),
        };
    }

    private static int HandleHelp()
    {
        WriteHelp(Console.Out);
        return ExitSuccess;
    }

    private static int HandleVersion()
    {
        Console.WriteLine("japp 0.1.0");
        return ExitSuccess;
    }

    private static int HandleRun(string[] args)
    {
        if (args.Length == 0 || args[0].StartsWith('-'))
        {
            Console.Error.WriteLine("Missing japp file name.");
            Console.Error.WriteLine("Run 'japp help' for usage.");
            return ExitUsage;
        }

        // Same shape as native/src/launcher.rs: JVM options first, then the
        // japp file. Both are currently uninterpreted.
        var jvmOptions = args.TakeWhile(arg => arg.StartsWith('-')).ToList();
        Console.Error.WriteLine($"TODO: run {args[jvmOptions.Count]} ({jvmOptions.Count} JVM options)");
        return ExitFailure;
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unsupported command: {command}");
        WriteHelp(Console.Error);
        return ExitUsage;
    }

    private static void WriteHelp(TextWriter writer)
    {
        writer.WriteLine("""
                          Usage: japp <command> [options]

                          Commands:
                            run [jvm options] <file.japp> [args]   Run a japp application
                            version                                Print the version
                            help                                  Print this help
                          """);
    }
}
