// Copyright © 2025 Roby Van Damme.

using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotAdr.Commands.Init;

internal class InitAdrSettings : AdrSettings
{
    [Description("The directory to initialize.")]
    [CommandOption("-d|--directory")]
    [DefaultValue("./doc/adr")]
    public string? Directory { get; init; }

    [Description("Path to a custom template to use for the initialization.")]
    [CommandOption("-t|--template")]
    public string? TemplatePath { get; init; }

    [Description("Whether to overwrite existing files.")]
    [CommandOption("-o|--overwrite")]
    [DefaultValue("false")]
    public bool Overwrite { get; init; }

    public override ValidationResult Validate()
    {
        if (!string.IsNullOrEmpty(TemplatePath) && !File.Exists(TemplatePath))
        {
            return ValidationResult.Error($"The template path '{TemplatePath}' does not exist.");
        }

        return ValidationResult.Success();
    }
}
