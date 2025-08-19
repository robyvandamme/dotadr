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

    public DecisionRecord CreateDecisionRecord(string templateContent, string id, string decisionTitle)
    {
        logger.MethodStart(nameof(AdrFactory), nameof(CreateDecisionRecord));

        var templateVariables = new Dictionary<string, string>
        {
            ["ID"] = id,
            ["TITLE"] = decisionTitle,
            ["DATE"] = DateOnly.FromDateTime(DateTime.Today).ToString("O", CultureInfo.InvariantCulture),
        };

        var decisionContent = ProcessTemplate(templateContent, templateVariables);

        logger.MethodReturn(nameof(AdrFactory), nameof(CreateDecisionRecord), decisionContent);

        return new DecisionRecord(id, decisionTitle, decisionContent);
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

        return template;
    }
}
