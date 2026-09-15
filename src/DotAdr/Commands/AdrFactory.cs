// Copyright © 2025 Roby Van Damme.

using System.Globalization;
using System.Text;
using DotAdr.Common;
using Serilog;

namespace DotAdr.Commands;

internal class AdrFactory(ILogger logger) : IAdrFactory
{
    public string CreateDecisionTemplate()
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionTemplate));
        var sb = new StringBuilder();
        sb.Append("# {{ID}} {{TITLE}}");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("* Status: Draft");
        sb.AppendLine();
        sb.Append("* Date: {{DATE}} ");
        sb.AppendLine();
        sb.Append("* Supersedes: {{SUPERSEDES}}");
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

    public DecisionRecord CreateDecisionRecord(
        string templateContent,
        string id,
        string decisionTitle,
        SupersededDecisionRecord? supersededDecisionRecord)
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionRecord));

        ArgumentException.ThrowIfNullOrWhiteSpace(templateContent);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionTitle);

        var templateVariables = new Dictionary<string, string>
        {
            ["ID"] = id,
            ["TITLE"] = decisionTitle,
            ["DATE"] = DateOnly.FromDateTime(DateTime.Today).ToString("O", CultureInfo.InvariantCulture),
        };

        if (supersededDecisionRecord != null)
        {
            templateVariables.Add(
                "SUPERSEDES",
                $"[{supersededDecisionRecord.Id}]({supersededDecisionRecord.FileName})");
        }

        var decisionContent = ProcessTemplate(templateContent, templateVariables);

        logger.MethodReturn(nameof(AdrFactory), nameof(CreateDecisionRecord), decisionContent);

        return new DecisionRecord(id, decisionTitle, decisionContent);
    }

    public string UpdateSupersededDecisionContent(
        SupersededDecisionRecord supersededDecisionRecord,
        DecisionRecord supersedingRecord,
        string supersedingFileName)
    {
        ArgumentNullException.ThrowIfNull(supersededDecisionRecord);
        ArgumentNullException.ThrowIfNull(supersedingRecord);
        ArgumentException.ThrowIfNullOrWhiteSpace(supersedingFileName);

        if (supersededDecisionRecord.Content.Contains("* Status:", StringComparison.OrdinalIgnoreCase))
        {
            var appendText =
                $" - Superseded by [{supersedingRecord.Id}]({supersedingFileName}) {DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            var updated = AppendToStatusLine(supersededDecisionRecord.Content, appendText);
            return updated;
        }

        return supersededDecisionRecord.Content;
    }

    private static string AppendToStatusLine(string content, string appendText)
    {
        var lineStart = 0;
        while (lineStart < content.Length)
        {
            var lineEnd = content.IndexOfAny(['\r', '\n'], lineStart);
            if (lineEnd < 0)
            {
                lineEnd = content.Length;
            }

            if (content[lineStart..lineEnd].Contains("* Status:", StringComparison.OrdinalIgnoreCase))
            {
                return content[..lineEnd] + appendText + content[lineEnd..];
            }

            if (lineEnd == content.Length)
            {
                break;
            }

            lineStart = lineEnd + 1;

            // Treat CRLF as one line ending.
            if (content[lineEnd] == '\r' &&
                lineEnd + 1 < content.Length &&
                content[lineEnd + 1] == '\n')
            {
                lineStart++;
            }
        }

        return content;
    }

    private static string ProcessTemplate(string template, Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            template = template.Replace(
                $"{{{{{variable.Key}}}}}",
                variable.Value,
                StringComparison.InvariantCultureIgnoreCase);
        }

        if (!variables.ContainsKey("SUPERSEDES"))
        {
            var lines = template.Split('\n');
            var processedLines = new List<string>();
            foreach (var line in lines)
            {
                // Remove any line containing the unresolved supersedes token.
                if (!line.Contains("{{SUPERSEDES}}", StringComparison.InvariantCultureIgnoreCase))
                {
                    processedLines.Add(line);
                }
            }

            return string.Join('\n', processedLines);
        }

        return template;
    }
}
