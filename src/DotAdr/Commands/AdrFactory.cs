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

    // TODO: we can add the Superseded record as an optional parameter in here. That should be
    // enough to handle the Supersedes section? 
    public DecisionRecord CreateDecisionRecord(
        string templateContent,
        string id,
        string decisionTitle,
        SupersededDecisionRecord? supersededDecisionRecord)
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionRecord));

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

    private string ProcessTemplate(string template, Dictionary<string, string> variables)
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
            foreach (string line in lines)
            {
                // Skip lines that still contain placeholder tokens
                if (!ContainsPlaceholder(line))
                {
                    processedLines.Add(line);
                }
            }

            return string.Join('\n', processedLines);
        }

        return template;
    }

    private bool ContainsPlaceholder(string line)
    {
        return line.Contains("{{", StringComparison.OrdinalIgnoreCase) &&
               line.Contains("}}", StringComparison.OrdinalIgnoreCase);
    }
}
