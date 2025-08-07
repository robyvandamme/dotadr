// Copyright © 2025 Roby Van Damme.

namespace DotAdr.Commands;

public interface IAdrFactory
{
    string CreateDecisionTemplate();

    DecisionRecord CreateDecisionRecord(string templateContent, string decisionTitle);
}
