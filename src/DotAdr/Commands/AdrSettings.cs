// Copyright © Roby Van Damme.

using System.ComponentModel;
using Spectre.Console.Cli;

namespace DotAdr.Commands;

/// <summary>
/// Base class for command settings.
/// </summary>
internal abstract class AdrSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether debug logging is enabled. The value is consumed by the
    /// <see cref="DotAdr.LoggingInterceptor"/> to configure Serilog before the command runs.
    /// </summary>
    [Description("Enable debug logging for troubleshooting")]
    [CommandOption("--debug")]
    [DefaultValue(false)]
    public bool Debug { get; set; }

    /// <summary>
    /// Gets or sets the file to send the log output to. The value is consumed by the
    /// <see cref="DotAdr.LoggingInterceptor"/> to configure Serilog before the command runs.
    /// </summary>
    [Description("The file to send the log output to.")]
    [CommandOption("--logfile")]
    public string? LogFile { get; set; }
}
