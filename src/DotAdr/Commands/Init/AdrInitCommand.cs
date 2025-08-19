// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotAdr.Commands.Init;

internal class AdrInitCommand(
    IAnsiConsole console,
    ILogger logger,
    IAdrFileService adrFileService,
    IAdrFactory adrFactory,
    IConfigurationService configurationService)
    : Command<AdrInitSettings>
{
    public override int Execute(CommandContext context, AdrInitSettings settings)
    {
        logger.MethodStart(nameof(AdrInitCommand), nameof(Execute));

        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            if (context.Name != "init")
            {
                throw new DotAdrException($"Unsupported command name {context.Name}");
            }

            var adrDirectoryPath = new LocalDirectory(settings.Directory ?? "./doc/adr");

            // Pass in the directory as is. We normalize it in the configuration service
            configurationService.SaveAdrConfiguration(adrDirectoryPath, settings.Overwrite);

            // Create the template
            var template = adrFactory.CreateDecisionTemplate();

            // Create the initial record
            var title = "Use Architectural Decision Records";
            var initialDecision = adrFactory.CreateDecisionRecord(template, "001", title);

            adrFileService.InitializeDirectory(adrDirectoryPath, template, initialDecision, settings.Overwrite);
            console.MarkupLine($"ADR directory {adrDirectoryPath.RelativePath} initialized");
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            logger.Error(e, "An error occured while trying to initialize the ADR directory");
            console.WriteException(e, ExceptionFormats.ShortenEverything);
            logger.MethodReturn(nameof(AdrInitCommand), nameof(Execute));
            return 1;
        }

        logger.MethodReturn(nameof(AdrInitCommand), nameof(Execute));
        return 0;
    }
}
