// Copyright © 2025 Roby Van Damme.

using System.Diagnostics;
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
        DecisionRecord initialDecisionRecord)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(InitializeDirectory));

        ArgumentNullException.ThrowIfNull(nameof(adrDirectory));
        ArgumentNullException.ThrowIfNull(nameof(initialDecisionRecord));
        ArgumentException.ThrowIfNullOrEmpty(nameof(decisionTemplate));

        // AdrFileService.cs(28, 47): [CA1062] In externally visible method 'void AdrFileService.InitializeDirectory(DotBotDirectory directory, string decisionTemplate, string initialDecisionRecord, string initialDecisionRecordTitle)', validate parameter 'directory' is non-null before using it. If appropriate, throw an 'ArgumentNullException' when the argument is 'null'. (https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1062)
        // ArgumentNullException.ThrowIfNull does not resolve the above warning....
        // Debug.Assert(adrDirectory != null, nameof(adrDirectory) + " != null");
        var directoryInfo = new DirectoryInfo(adrDirectory.AbsolutePath);
        if (!directoryInfo.Exists)
        {
            logger.Debug("Creating directory at {Directory}", adrDirectory.AbsolutePath);
            directoryInfo.Create();
        }
        else
        {
            // TODO: throw or not throw? I think we need to throw. Minimize issues.
            // But, that might mean we have a config but no template or initial record....
            logger.Debug("Directory already exists: {Directory}", adrDirectory.AbsolutePath);
        }

        var templateFilePath = Path.Combine(adrDirectory.AbsolutePath, _templateName);
        var templateFileInfo = new FileInfo(templateFilePath);
        if (!templateFileInfo.Exists)
        {
            logger.Debug("Writing template to {FilePath}", templateFilePath);
            File.WriteAllText(templateFilePath, decisionTemplate);
        }
        else
        {
            // TODO: throw or not throw? Minimize issues.
            logger.Debug("File already exists: {FilePath}", templateFilePath);
        }

        CreateInitialDecisionRecord(adrDirectory, initialDecisionRecord);

        logger.MethodReturn(nameof(AdrFileService), nameof(InitializeDirectory));
    }

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
        return templateContent;
    }

    public string AddDecisionRecord(LocalDirectory directory, DecisionRecord decisionRecord)
    {
        logger.MethodStart(nameof(AdrFileService), nameof(AddDecisionRecord));

        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(decisionRecord);

        logger.Debug("AddDecisionRecord in directory {Directory}", directory);

        var info = new DirectoryInfo(directory.AbsolutePath);
        if (!info.Exists)
        {
            throw new DotAdrException($"The directory {directory} does not exist");
        }

        // TODO: we need this info for the adr factory...
        var nextNumber = GetNextFileNumber(directory.AbsolutePath);

        // Create a file name friendly version of the title
        var safeTitle = MakeSafeFileName(decisionRecord.Title);

        var fileName = $"{nextNumber:000}-{safeTitle}.md";

        // Create the new file path with the numbering prefix
        var newFilePath = Path.Combine(directory.AbsolutePath, fileName);

        // Write the updated content to the new file
        File.WriteAllText(newFilePath, decisionRecord.Content);

        return fileName;
    }

    private static int GetNextFileNumber(string normalizedDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedDirectoryPath);

        // Get all existing markdown files that follow our naming convention
        var files = Directory.GetFiles(normalizedDirectoryPath, "???-*.md")
            .Select(Path.GetFileName)
            .Where(file => file != null && Regex.IsMatch(file, @"^\d{3}-.*\.md$"))
            .ToList();

        if (files.Count == 0)
        {
            // No existing files, start with 001
            return 1;
        }

        // Extract the highest number and increment it
        var highestNumber = files
            .Select(f => int.Parse(f.AsSpan(0, 3), CultureInfo.InvariantCulture))
            .Max();

        return highestNumber + 1;
    }

    private static string MakeSafeFileName(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        // Replace invalid file name characters and spaces
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

        return safe;
    }

    private void CreateInitialDecisionRecord(LocalDirectory adrDirectory, DecisionRecord initialDecisionRecord)
    {
        var number = GetNextFileNumber(adrDirectory.AbsolutePath);
        if (number != 1)
        {
            logger.Debug(
                "A decision record with number {Number} already exists in the directory{Directory}",
                number,
                adrDirectory.AbsolutePath);
        }
        else
        {
            logger.Debug("Creating initial decision record in {Directory}", adrDirectory.AbsolutePath);
            AddDecisionRecord(adrDirectory, initialDecisionRecord);
        }
    }
}
