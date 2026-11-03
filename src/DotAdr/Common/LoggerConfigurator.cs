// Copyright © Roby Van Damme.

using System.Globalization;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DotAdr.Common;

internal static class LoggerConfigurator
{
    internal static void Configure(bool debug, string? logFile)
    {
        var defaultLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Error);

        if (debug)
        {
            defaultLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        }

        if (!string.IsNullOrWhiteSpace(logFile))
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(defaultLevelSwitch)
#if DEBUG
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
#endif
                .WriteTo.File(
                    logFile,
                    rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(defaultLevelSwitch)
#if DEBUG
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
#endif
                .CreateLogger();
        }
    }
}
