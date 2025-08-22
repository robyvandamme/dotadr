// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotAdr.Commands.Add;

internal class AddAdrCommand(
    IAnsiConsole console,
    ILogger logger,
    IAdrFileService adrFileService,
    IAdrFactory adrFactory,
    IConfigurationService configurationService)
    : Command<AddAdrSettings>
{
    public override int Execute(CommandContext context, AddAdrSettings settings)
    {
        logger.MethodStart(nameof(AddAdrCommand), nameof(Execute));

        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            if (context.Name != "add")
            {
                throw new DotAdrException($"Unsupported command name {context.Name}");
            }

            logger.Debug("Running {Command} with settings {@Settings}", nameof(AddAdrCommand), settings);

            var config = configurationService.GetDotAdrConfiguration();
            var adrDirectory = new LocalDirectory(config.Directory);
            var template = adrFileService.GetTemplate(adrDirectory);
            var nextId = adrFileService.GetNextRecordId(adrDirectory);

            SupersededDecisionRecord? superseded = null;

            if (!string.IsNullOrWhiteSpace(settings.Supersedes))
            {
                superseded = adrFileService.TryFindSupersededDecisionRecord(settings.Supersedes, adrDirectory);
            }

            var record = adrFactory.CreateDecisionRecord(template, nextId, settings.Title, superseded);
            var fileName = adrFileService.AddDecisionRecord(adrDirectory, record);

            if (superseded != null)
            {
                var content = adrFactory.UpdateSupersededDecisionContent(superseded, record, fileName);
                adrFileService.SaveSupersedeDecisionRecord(adrDirectory, superseded, content);
            }

            console.MarkupLine($"{fileName} added to the {adrDirectory.RelativePath} directory");
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            logger.Error(e, "An error occured while trying to add the decision record");
            console.WriteException(e, ExceptionFormats.ShortenEverything);
            logger.MethodReturn(nameof(AddAdrCommand), nameof(Execute));
            return 1;
        }

        logger.MethodReturn(nameof(AddAdrCommand), nameof(Execute));
        return 0;
    }
}
