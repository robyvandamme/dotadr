// Copyright © 2025 Roby Van Damme.

using System.ComponentModel;
using Spectre.Console.Cli;

namespace DotAdr.Commands.Init;

internal class AdrInitSettings : LogSettings
{
    [Description("The directory to initialize. Defaults to `./doc/adr`")]
    [CommandOption("-d|--directory")]
    [DefaultValue("./doc/adr")]
    public string? Directory { get; init; }

    [Description("Overwrite existing files? Defaults to false.")]
    [CommandOption("-o|--overwrite")]
    [DefaultValue(false)]
    public bool Overwrite { get; init; }
}
