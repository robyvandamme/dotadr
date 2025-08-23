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

    string GetNextRecordId(LocalDirectory adrDirectory);

    string AddDecisionRecord(LocalDirectory adrDirectory, DecisionRecord decisionRecord);

    SupersededDecisionRecord? TryFindSupersededDecisionRecord(string id, LocalDirectory adrDirectory);

    void SaveSupersedeDecisionRecord(
        LocalDirectory adrDirectory,
        SupersededDecisionRecord decisionRecord,
        string updatedContent);
}
