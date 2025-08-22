// Copyright © 2025 Roby Van Damme.

using System.Globalization;
using System.Text.RegularExpressions;
using DotAdr.Common;
using Serilog;

namespace DotAdr.Commands;

internal class AdrFileService(ILogger logger) : IAdrFileService
{
    private readonly string _templateName = "template.md";

    public void InitializeDirectory(
        LocalDirectory adrDirectory,
        string decisionTemplate,
        DecisionRecord initialDecisionRecord,
        bool overwriteFiles)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(InitializeDirectory));

        ArgumentNullException.ThrowIfNull(nameof(adrDirectory));
        ArgumentNullException.ThrowIfNull(nameof(initialDecisionRecord));
        ArgumentException.ThrowIfNullOrEmpty(nameof(decisionTemplate));

        CreateDirectory(adrDirectory);

        AddTemplate(adrDirectory, decisionTemplate, overwriteFiles);

        AddInitialDecisionRecord(adrDirectory, initialDecisionRecord, overwriteFiles);

        logger.MethodReturn(nameof(AdrFileService), nameof(InitializeDirectory));
    }

    public string AddDecisionRecord(LocalDirectory adrDirectory, DecisionRecord decisionRecord)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(AddDecisionRecord));

        ArgumentNullException.ThrowIfNull(adrDirectory);
        ArgumentNullException.ThrowIfNull(decisionRecord);

        logger.Debug("AddDecisionRecord in directory {Directory}", adrDirectory);

        var info = new DirectoryInfo(adrDirectory.AbsolutePath);
        if (!info.Exists)
        {
            throw new DotAdrException($"The directory {adrDirectory} does not exist");
        }

        // Create a file name friendly version of the title
        var safeTitle = MakeSafeFileName(decisionRecord.Title);

        var fileName = $"{decisionRecord.Id}-{safeTitle}.md";

        // Create the new file path with the numbering prefix
        var newFilePath = Path.Combine(adrDirectory.AbsolutePath, fileName);

        // Write the updated content to the new file
        File.WriteAllText(newFilePath, decisionRecord.Content);

        logger.MethodReturn(nameof(AdrFileService), nameof(AddDecisionRecord), fileName);

        return fileName;
    }

    /// <summary>
    /// Gets the next record ID based on the files ih the ADR directory.
    /// </summary>
    /// <param name="adrDirectory">The ADR directory. </param>
    /// <returns>The next record id in the "xxx" format.</returns>
    public string GetNextRecordId(LocalDirectory adrDirectory)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(GetNextRecordId));

        ArgumentNullException.ThrowIfNull(adrDirectory);

        // Get all existing markdown files that follow our naming convention
        var files = Directory.GetFiles(adrDirectory.AbsolutePath, "???-*.md")
            .Select(Path.GetFileName)
            .Where(file => file != null && Regex.IsMatch(file, @"^\d{3}-.*\.md$"))
            .ToList();

        if (files.Count == 0)
        {
            // No existing files, start with 001. This should not happen, unless you delete the initial decision record.
            return "001";
        }

        // Extract the highest number
        var highestNumber = files
            .Select(f => int.Parse(f.AsSpan(0, 3), CultureInfo.InvariantCulture))
            .Max();

        logger.MethodReturn(nameof(AdrFileService), nameof(GetNextRecordId), highestNumber);

        return $"{highestNumber + 1:000}";
    }

    /// <summary>
    /// Gets the template content from the template.md file in the ADR directory.
    /// </summary>
    /// <param name="adrDirectory">The ADR directory.</param>
    /// <returns>The template content string.</returns>
    /// <exception cref="DotAdrException">When the file of directory does not exist.</exception>
    public string GetTemplate(LocalDirectory adrDirectory)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(GetTemplate));

        ArgumentNullException.ThrowIfNull(adrDirectory);

        logger.Debug("GetTemplate with directory {Directory}", adrDirectory.AbsolutePath);

        var info = new DirectoryInfo(adrDirectory.AbsolutePath);
        if (!info.Exists)
        {
            throw new DotAdrException($"The directory {adrDirectory.AbsolutePath} does not exist");
        }

        var templateFilePath = Path.Combine(adrDirectory.AbsolutePath, _templateName);
        var fileInfo = new FileInfo(templateFilePath);
        if (!fileInfo.Exists)
        {
            throw new DotAdrException($"The file {templateFilePath} does not exist");
        }

        var templateContent = File.ReadAllText(templateFilePath);
        logger.MethodReturn(nameof(AdrFileService), nameof(GetTemplate), templateContent);
        return templateContent;
    }

    /// <summary>
    /// Tries to find the superseded decision record.
    /// </summary>
    /// <param name="id">The record to find.</param>
    /// <param name="adrDirectory">The ADR directory.</param>
    /// <returns>The <see cref="SupersededDecisionRecord"/> if found.</returns>
    /// <exception cref="DotAdrException">When no record is found.</exception>
    public SupersededDecisionRecord? TryFindSupersededDecisionRecord(string id, LocalDirectory adrDirectory)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(TryFindSupersededDecisionRecord));

        var markdownFiles = Directory.GetFiles(adrDirectory.AbsolutePath, "*.md");
        var supersededFile =
            markdownFiles.FirstOrDefault(o => Path.GetFileName(o).StartsWith(id, StringComparison.OrdinalIgnoreCase));
        if (supersededFile != null)
        {
            var content = File.ReadAllText(supersededFile);
            var superseded = new SupersededDecisionRecord(id, Path.GetFileName(supersededFile), content);
            logger.MethodReturn(nameof(AdrFileService), nameof(TryFindSupersededDecisionRecord), superseded);
            return superseded;
        }

        // If this method is called, which should only be in the case of an -s command option,
        // and we can not find the superseded record, we throw for now. That seems like the easiest option.
        // We can move this to Spectre validation later on.
        logger.Debug("No file with ID {id} found in {@directory}", id, adrDirectory);
        throw new DotAdrException($"A record with ID {id} could not be found in the directory {@adrDirectory}");
    }

    /// <summary>
    /// Saves the superseded decision record.
    /// </summary>
    /// <param name="adrDirectory">The ADR directory.</param>
    /// <param name="decisionRecord">The decision record.</param>
    /// <param name="updatedContent">The updated content.</param>
    /// <exception cref="DotAdrException">When the decision record does not exist.</exception>
    public void SaveSupersedeDecisionRecord(
        LocalDirectory adrDirectory,
        SupersededDecisionRecord decisionRecord,
        string updatedContent)
    {
        var filePath = Path.Combine(adrDirectory.AbsolutePath, decisionRecord.FileName);
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            throw new DotAdrException($"There is no superseded record file at {fileInfo}");
        }

        logger.Debug("Writing template to {FilePath}", filePath);
        File.WriteAllText(filePath, updatedContent);
    }

    private void AddInitialDecisionRecord(
        LocalDirectory adrDirectory,
        DecisionRecord initialDecisionRecord,
        bool overwriteFile)
    {
        var safeTitle = MakeSafeFileName(initialDecisionRecord.Title);
        var fileName = $"{initialDecisionRecord.Id}-{safeTitle}.md";
        var filePath = Path.Combine(adrDirectory.AbsolutePath, fileName);

        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists || overwriteFile)
        {
            File.WriteAllText(filePath, initialDecisionRecord.Content);
        }
        else
        {
            Log.Debug("The file {file} already exists", filePath);
        }
    }

    private string MakeSafeFileName(string title)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(MakeSafeFileName));

        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var safe = title.Trim();

        // Replace invalid characters with empty strings
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            safe = safe.Replace(c.ToString(), string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        // Replace spaces with dashes and make lowercase for consistency
#pragma warning disable CA1308
        safe = safe.Replace(" ", "-", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
#pragma warning restore CA1308

        // Remove multiple consecutive dashes
        safe = Regex.Replace(safe, @"-+", "-");

        logger.MethodReturn(nameof(AdrFileService), nameof(MakeSafeFileName), safe);

        return safe;
    }

    private void CreateDirectory(LocalDirectory adrDirectory)
    {
        var directoryInfo = new DirectoryInfo(adrDirectory.AbsolutePath);
        if (!directoryInfo.Exists)
        {
            logger.Debug("Creating directory at {Directory}", adrDirectory.AbsolutePath);
            directoryInfo.Create();
        }
        else
        {
            logger.Debug("Directory already exists: {Directory}", adrDirectory.AbsolutePath);
        }
    }

    private void AddTemplate(LocalDirectory adrDirectory, string decisionTemplate, bool overwriteFiles)
    {
        var templateFilePath = Path.Combine(adrDirectory.AbsolutePath, _templateName);
        var templateFileInfo = new FileInfo(templateFilePath);
        if (!templateFileInfo.Exists || overwriteFiles)
        {
            logger.Debug("Writing template to {FilePath}", templateFilePath);
            File.WriteAllText(templateFilePath, decisionTemplate);
        }
        else
        {
            logger.Debug("File already exists: {FilePath}", templateFilePath);
        }
    }
}
