// Copyright © 2025 Roby Van Damme.

using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotAdr.Commands.Add;

internal class AddAdrSettings : AdrSettings
{
    [Description("The title of the new decision record.")]
    [CommandArgument(0, "<title>")]
    public required string Title { get; set; }

    [Description("The ID of the decision record this decision record supersedes.")]
    [CommandOption("-s|--supersedes")]
    public string? Supersedes { get; set; }

    [Description("Path to a custom template to use for the new decision record.")]
    [CommandOption("-t|--template")]
    public string? TemplatePath { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            return ValidationResult.Error("The title is required.");
        }

        if (!string.IsNullOrEmpty(TemplatePath) && !File.Exists(TemplatePath))
        {
            return ValidationResult.Error($"The template path '{TemplatePath}' does not exist.");
        }

        return ValidationResult.Success();
    }
}
