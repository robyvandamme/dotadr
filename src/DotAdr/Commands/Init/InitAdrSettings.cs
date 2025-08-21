// Copyright © 2025 Roby Van Damme.

using System.ComponentModel;
using Spectre.Console.Cli;

namespace DotAdr.Commands.Init;

internal class InitAdrSettings : AdrSettings
{
    [Description("The directory to initialize.")]
    [CommandOption("-d|--directory")]
    [DefaultValue("./doc/adr")]
    public string? Directory { get; init; }

    [Description("Whether to overwrite existing files.")]
    [CommandOption("-o|--overwrite")]
    [DefaultValue("false")]
    public bool Overwrite { get; init; }
}
