// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;

namespace DotAdr.Commands;

public interface IAdrFileService
{
    /// <summary>
    /// Get the template from the ADR directory.
    /// </summary>
    /// <param name="adrDirectory">The ADR directory.</param>
    /// <returns>The template string.</returns>
    string GetTemplate(LocalDirectory adrDirectory);

    /// <summary>
    /// Initialize the specified ADR directory with a default template and an initial decision record.
    /// </summary>
    /// <param name="adrDirectory">The ADR directory.</param>
    /// <param name="decisionTemplate">The decision template string.</param>
    /// <param name="initialDecisionRecord">The initial decision record to create.</param>
    void InitializeDirectory(
        LocalDirectory adrDirectory,
        string decisionTemplate,
        DecisionRecord initialDecisionRecord);

    /// <summary>
    /// Add a decision record to the specified directory.
    /// </summary>
    /// <param name="directory">The directory to use.</param>
    /// <param name="decisionRecord">The decision record to add.</param>
    /// <returns>The file name of the record.</returns>
    string AddDecisionRecord(LocalDirectory directory, DecisionRecord decisionRecord);
}
