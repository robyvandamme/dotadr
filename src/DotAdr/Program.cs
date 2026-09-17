// Copyright © 2025 Roby Van Damme.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using DotAdr;
using DotAdr.Common;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Spectre.Console;
using Spectre.Console.Cli;

#if DEBUG
Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif

ConfigureLogger();

var versionInfo = new VersionInfo(Assembly.GetExecutingAssembly());
var platform = GetPlatform();

Log.Debug(
    "DotADR {Version} running on {Runtime}, {Platform} {Architecture} (OS details: {OSDescription})",
    versionInfo.Version,
    RuntimeInformation.FrameworkDescription,
    platform,
    RuntimeInformation.ProcessArchitecture,
    RuntimeInformation.OSDescription);
Log.Debug("Configuring app");

var commandApp = new CommandApp();

commandApp.Configure(Log.Logger);

try
{
    Log.Debug("Starting app");
    return await commandApp.RunAsync(args);
}
#pragma warning disable CA1031
catch (Exception ex)
#pragma warning restore CA1031
{
    Log.Error(ex, "An error occurred");
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

void ConfigureLogger()
{
    var defaultLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Error);
    if (ArgumentHandler.IsDebugMode(args))
    {
        defaultLevelSwitch.MinimumLevel = LogEventLevel.Debug;
    }

    var logFile = ArgumentHandler.LogFile(args);

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

string GetPlatform()
{
    var platformString = OperatingSystem.IsWindows()
        ? "Windows"
        : OperatingSystem.IsMacOS()
            ? "macOS"
            : OperatingSystem.IsLinux()
                ? "Linux"
                : OperatingSystem.IsFreeBSD()
                    ? "FreeBSD"
                    : "Unknown OS";
    return platformString;
}
