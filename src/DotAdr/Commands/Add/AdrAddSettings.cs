// Copyright © 2025 Roby Van Damme.

using System.ComponentModel;
using Spectre.Console.Cli;

namespace DotAdr.Commands.Add;

internal class AdrAddSettings : AdrSettings
{
    [Description("The title of the new decision record.")]
    [CommandArgument(0, "[title]")]
    public required string Title { get; set; }
}
