// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;

namespace DotAdr.Commands;

internal interface IAdrFileService
{
    string GetTemplate(LocalDirectory adrDirectory);

    void InitializeDirectory(
        LocalDirectory adrDirectory,
        string decisionTemplate,
        DecisionRecord initialDecisionRecord,
        bool overwriteFiles);

    string GetNextRecordId(LocalDirectory localDirectory);

    string AddDecisionRecord(LocalDirectory directory, DecisionRecord decisionRecord);

    SupersededDecisionRecord? TryGetSupersededDecisionRecord(string id, LocalDirectory adrDirectory);
}
