// Copyright © 2025 Roby Van Damme.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DotAdr.Common;
using Serilog;

namespace DotAdr.Commands;

internal class AdrFactory(ILogger logger) : IAdrFactory
{
    public string CreateDecisionTemplate()
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionTemplate));
        var sb = new StringBuilder();
        sb.Append("# ADR Template");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("---");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("Status: Draft");
        sb.AppendLine();
        sb.AppendLine();
#pragma warning disable CA1305
        sb.Append($"Date: {DateOnly.FromDateTime(DateTime.MinValue).ToString("O", CultureInfo.InvariantCulture)}");
#pragma warning restore CA1305
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("Authors: ");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("---");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("## Context");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("## Decision");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("## Consequences");

        logger.MethodReturn(nameof(AdrFactory), nameof(CreateDecisionTemplate), sb.ToString());
        return sb.ToString();
    }

    public DecisionRecord CreateDecisionRecord(string templateContent, string decisionTitle)
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionRecord));

        // TODO: we also need the decision record number here.
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
