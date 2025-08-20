// Copyright © 2025 Roby Van Damme.

namespace DotAdr.Commands;

internal interface IAdrFactory
{
    string CreateDecisionTemplate();

    DecisionRecord CreateDecisionRecord(string templateContent, string id, string decisionTitle);
}
