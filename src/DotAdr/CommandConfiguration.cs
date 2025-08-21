// Copyright © 2025 Roby Van Damme.

using System.Diagnostics;
using DotAdr.Commands;
using DotAdr.Commands.Add;
using DotAdr.Commands.Init;
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

            config.Settings.Registrar.Register<IConfigurationService, ConfigurationService>();
            config.Settings.Registrar.Register<IAdrFileService, AdrFileService>();
            config.Settings.Registrar.Register<IAdrFactory, AdrFactory>();

            config.AddCommand<InitAdrCommand>(name: "init")
                .WithDescription("Initialize the ADR directory.")
                .WithExample("init")
                .WithExample("init", "-d", "./doc/arch/adr", "-o", "true")
                .WithExample("init", "--debug", "true", "--logfile", "log.txt");
            config.AddCommand<AddAdrCommand>(name: "add")
                .WithAlias("new")
                .WithDescription("Add a new decision record.")
                .WithExample("add", "\"Title of the new decision record\"")
                .WithExample("add", "\"Title of the new decision record\"", "--debug", "true", "--logfile", "log.txt")
                .WithExample("new", "\"Title of the new decision record\"");
        });
    }
}
