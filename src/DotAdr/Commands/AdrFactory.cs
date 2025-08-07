// Copyright © 2025 Roby Van Damme.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DotAdr.Common;
using Serilog;

namespace DotAdr.Commands;

public class AdrFactory(ILogger logger) : IAdrFactory
{
    public string CreateDecisionTemplate()
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionTemplate));
        var sb = new StringBuilder();
        // using (var writer = MarkdownWriter.Create(sb))
        // {
        //     writer.WriteHeading1("ADR Template");
        //     writer.WriteRaw(
        //         $"Date: {DateOnly.FromDateTime(DateTime.MinValue).ToString("O", CultureInfo.InvariantCulture)}");
        //     writer.WriteLine();
        //     writer.WriteHeading2("Status");
        //     writer.WriteString("Proposed");
        //     writer.WriteHeading2("Context");
        //     writer.WriteHeading2("Decision");
        //     writer.WriteHeading2("Consequences");
        // }

        logger.MethodReturn(nameof(AdrFactory), nameof(CreateDecisionTemplate), sb.ToString());
        return sb.ToString();
    }

    public DecisionRecord CreateDecisionRecord(string templateContent, string decisionTitle)
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionRecord));

        var newDateString = DateOnly.FromDateTime(DateTime.Today).ToString("O", CultureInfo.InvariantCulture);

        // Replace the header (assumes the header is in Markdown format like "# Title")
        var updatedContent = Regex.Replace(
            templateContent,
            @"^# .*$",
            $"# {decisionTitle}",
            RegexOptions.Multiline);

        // Replace the date string (assuming it's in a format like "Date: YYYY-MM-DD")
        updatedContent = Regex.Replace(
            updatedContent,
            @"Date:.*\d{4}-\d{2}-\d{2}",
            $"Date: {newDateString}");

        logger.MethodReturn(nameof(AdrFactory), nameof(CreateDecisionRecord), updatedContent);

        return new DecisionRecord(decisionTitle, updatedContent);
    }
}
