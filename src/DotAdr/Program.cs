// Copyright © Roby Van Damme.

using System.Diagnostics;
using System.Reflection;
using DotAdr;
using DotAdr.Common;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

#if DEBUG
Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif

var versionInfo = new VersionInfo(Assembly.GetExecutingAssembly());

var commandApp = new CommandApp();

commandApp.Configure(new DelegatingLogger(), versionInfo);

try
{
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
