// Copyright © 2025 Roby Van Damme.

namespace DotAdr.Commands;

internal interface IAdrFactory
{
    string CreateDecisionTemplate();

    DecisionRecord CreateDecisionRecord(int id, string templateContent, string decisionTitle);
}
