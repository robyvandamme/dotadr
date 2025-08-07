// Copyright © 2025 Roby Van Damme.

using System.Diagnostics;
using Serilog;
using Spectre.Console.Cli;

namespace DotAdr;

internal static class CommandConfiguration
{
    internal static void Configure(this CommandApp commandApp, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(nameof(commandApp));
        ArgumentNullException.ThrowIfNull(nameof(logger));

        Debug.Assert(commandApp != null, nameof(commandApp) + " != null");
        commandApp.Configure(config =>
        {
#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif
            config.SetApplicationName("dotadr");
            config.Settings.Registrar.RegisterInstance(logger);


            // config.AddCommand<BumpSdkCommand>(name: "sdk")
            //     .WithDescription(
            //         "Bump the global.json SDK version. " +
            //         "Use the 'minor' type option to bump the SDK to the latest minor or patch version for the current major version. " +
            //         "Use the 'patch' type option to bump the SDK to the latest patch version for the current major version. ")
            //     .WithExample("sdk", "-o", "bump-sdk-result.json", "--security-only", "true")
            //     .WithExample("sdk", "-t", "patch", "-o", "bump-sdk-result.json", "-s", "true")
            //     .WithExample("sdk", "--type", "patch", "-f", "./other/global.json")
            //     .WithExample("sdk", "--debug", "true", "--logfile", "log.txt");
        });
    }
}
