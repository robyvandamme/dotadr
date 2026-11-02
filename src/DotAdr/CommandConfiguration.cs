// Copyright © Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Commands.Add;
using DotAdr.Commands.Init;
using DotAdr.Common;
using Serilog;
using Spectre.Console.Cli;

namespace DotAdr;

internal static class CommandConfiguration
{
    internal static void Configure(this CommandApp commandApp, ILogger logger, VersionInfo versionInfo)
    {
        ArgumentNullException.ThrowIfNull(commandApp);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(versionInfo);

        commandApp.Configure(config =>
        {
#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif
            config.SetApplicationName("dotadr");
            config.SetApplicationVersion(versionInfo.Version);
            config.Settings.Registrar.RegisterInstance(logger);

            config.Settings.Registrar.Register<IConfigurationService, ConfigurationService>();
            config.Settings.Registrar.Register<IAdrFileService, AdrFileService>();
            config.Settings.Registrar.Register<IAdrFactory, AdrFactory>();

            config.AddCommand<InitAdrCommand>(name: "init")
                .WithDescription("Initialize the ADR directory.")
                .WithExample("init")
                .WithExample("init", "-d", "./doc/arch/adr", "-o")
                .WithExample("init", "-t", "./doc/templates/adr-template.md")
                .WithExample("init", "--debug", "--logfile", "log.txt");
            config.AddCommand<AddAdrCommand>(name: "add")
                .WithAlias("new")
                .WithDescription("Add a new decision record. Alias: new")
                .WithExample("add", "\"Implement Circuit Breaker Pattern for External Service Calls\"")
                .WithExample("add", "\"Separate Read and Write Data Models\"", "-s", "002")
                .WithExample("add", "\"Use a Custom ADR Template\"", "-t", "./doc/templates/adr-template.md")
                .WithExample("add", "\"Use Database Per Service Pattern\"", "--debug", "--logfile", "log.txt")
                .WithExample("new", "\"Implement Request Rate Limiting\"");
        });
    }
}
