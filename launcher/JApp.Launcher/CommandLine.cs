/*
 * Copyright (C) 2023 Glavo
 * Copyright (C) 2026 ghostflyby
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// Ported from src/main/java/org/glavo/japp/Main.java (mode dispatch) and the
// argument loop of src/main/java/org/glavo/japp/launcher/Launcher.java. Exit
// codes follow the Java launcher: 0 for help/success, 1 for usage errors and
// no-suitable-Java; unlike the Java loop, positional arguments after the japp
// file are forwarded to the application (the documented intent, matching
// EmbeddedLauncher) instead of overwriting the file name.

namespace JApp.Launcher;

internal static class CommandLine
{
    public static int Run(string[] args)
    {
        try
        {
            return RunInternal(args);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("Error: " + exception.Message);
            return 1;
        }
    }

    private static int RunInternal(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelpMessage(Console.Out);
            return 0;
        }

        var commandArgs = args[1..];

        switch (args[0])
        {
            case "help":
            case "-help":
            case "--help":
                PrintHelpMessage(Console.Out);
                return 0;

            case "version":
            case "-version":
            case "--version":
                Console.WriteLine("japp 0.1.0");
                return 0;

            case "run":
                return RunCommand(commandArgs);

            case "list-java":
                ListJava();
                return 0;

            default:
                Console.Error.WriteLine("Unsupported mode: " + args[0]);
                PrintHelpMessage(Console.Error);
                return 1;
        }
    }

    private static int RunCommand(string[] args)
    {
        var jvmOptions = new List<string>();
        string? jappFile = null;
        var appArgs = new List<string>();

        foreach (var arg in args)
        {
            if (jappFile is not null)
            {
                appArgs.Add(arg);
                continue;
            }

            switch (arg)
            {
                case "-help":
                case "--help":
                    PrintRunHelpMessage(Console.Out);
                    return 0;

                default:
                    if (arg.StartsWith("-J", StringComparison.Ordinal))
                        jvmOptions.Add(arg[2..]);
                    else if (arg.StartsWith('-'))
                    {
                        Console.Error.WriteLine("Unknown option: " + arg);
                        return 1;
                    }
                    else
                        jappFile = arg;
                    break;
            }
        }

        if (jappFile is null)
        {
            PrintRunHelpMessage(Console.Error);
            return 1;
        }

        return Launcher.Run(Path.GetFullPath(jappFile), jvmOptions, appArgs);
    }

    private static void ListJava()
    {
        var javas = JavaFinder.LocateAll();
        if (javas.Count == 0)
        {
            Console.WriteLine("Java runtime not found");
            return;
        }

        Console.WriteLine("Java runtime found:");
        foreach (var (jvm, context) in javas)
            Console.WriteLine($"  - {jvm.Executable.Path} ({context})");
    }

    private static void PrintHelpMessage(TextWriter writer)
    {
        writer.WriteLine("Usage: japp <mode> [options]");
        writer.WriteLine("Supported mode:");
        writer.WriteLine("  japp run");
        writer.WriteLine("  japp list-java");
    }

    private static void PrintRunHelpMessage(TextWriter writer)
    {
        writer.WriteLine("Usage: japp run [options] <japp file> [args]");
        writer.WriteLine("Supported options:");
        writer.WriteLine("  --help      Print this message");
        writer.WriteLine("  -J<flag>    Pass <flag> directly to java");
    }
}
