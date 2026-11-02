// Copyright © Roby Van Damme.

using System.Runtime.InteropServices;
using DotAdr.Commands;
using DotAdr.Common;
using Serilog;
using Spectre.Console.Cli;

namespace DotAdr;

/// <summary>
/// Configures logging from the settings Spectre has bound, before the command is created and executed.
/// </summary>
internal sealed class LoggingInterceptor(VersionInfo versionInfo) : ICommandInterceptor
{
    /// <inheritdoc/>
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(settings);

        if (settings is not AdrSettings adrSettings)
        {
            return;
        }

        LoggerConfigurator.Configure(adrSettings.Debug, adrSettings.LogFile);

        var platform = PlatformInfo.GetPlatform();

        Log.Debug(
            "DotADR {Version} running on {Runtime}, {Platform} {Architecture} (OS details: {OSDescription})",
            versionInfo.Version,
            RuntimeInformation.FrameworkDescription,
            platform,
            RuntimeInformation.ProcessArchitecture,
            RuntimeInformation.OSDescription);
        Log.Debug("Starting {Command}", context.Name);
    }
}
