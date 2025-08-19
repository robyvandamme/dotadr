// Copyright © 2025 Roby Van Damme.

using System.Diagnostics;
using System.Globalization;
using System.Text;
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

        CreateTemplate(adrDirectory, decisionTemplate, overwriteFiles);

        CreateInitialDecisionRecord(adrDirectory, initialDecisionRecord, overwriteFiles);

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

    private void CreateTemplate(LocalDirectory adrDirectory, string decisionTemplate, bool overwriteFiles)
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

    private void CreateInitialDecisionRecord(
        LocalDirectory adrDirectory,
        DecisionRecord initialDecisionRecord,
        bool overwriteFile)
    {
        var safeTitle = MakeSafeFileName(initialDecisionRecord.Title);
        var fileName = $"{initialDecisionRecord.Id:000}-{safeTitle}.md";
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

    // public string AddDecisionRecord(LocalDirectory directory, DecisionRecord decisionRecord)
    // {
    //     logger.MethodStart(nameof(AdrFileService), nameof(AddDecisionRecord));
    //
    //     ArgumentNullException.ThrowIfNull(directory);
    //     ArgumentNullException.ThrowIfNull(decisionRecord);
    //
    //     logger.Debug("AddDecisionRecord in directory {Directory}", directory);
    //
    //     var info = new DirectoryInfo(directory.AbsolutePath);
    //     if (!info.Exists)
    //     {
    //         throw new DotAdrException($"The directory {directory} does not exist");
    //     }
    //
    //     // TODO: we need this info for the adr factory...
    //     var nextNumber = GetNextFileNumber(directory.AbsolutePath);
    //
    //     // Create a file name friendly version of the title
    //     var safeTitle = MakeSafeFileName(decisionRecord.Title);
    //
    //     var fileName = $"{nextNumber:000}-{safeTitle}.md";
    //
    //     // Create the new file path with the numbering prefix
    //     var newFilePath = Path.Combine(directory.AbsolutePath, fileName);
    //
    //     // Write the updated content to the new file
    //     File.WriteAllText(newFilePath, decisionRecord.Content);
    //
    //     return fileName;
    // }

    // private static int GetNextFileNumber(string normalizedDirectoryPath)
    // {
    //     ArgumentException.ThrowIfNullOrWhiteSpace(normalizedDirectoryPath);
    //
    //     // Get all existing markdown files that follow our naming convention
    //     var files = Directory.GetFiles(normalizedDirectoryPath, "???-*.md")
    //         .Select(Path.GetFileName)
    //         .Where(file => file != null && Regex.IsMatch(file, @"^\d{3}-.*\.md$"))
    //         .ToList();
    //
    //     if (files.Count == 0)
    //     {
    //         // No existing files, start with 001
    //         return 1;
    //     }
    //
    //     // Extract the highest number and increment it
    //     var highestNumber = files
    //         .Select(f => int.Parse(f.AsSpan(0, 3), CultureInfo.InvariantCulture))
    //         .Max();
    //
    //     return highestNumber + 1;
    // }
}
