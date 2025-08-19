// Copyright © 2025 Roby Van Damme.

using System.Text.Json;
using DotAdr.Common;
using Serilog;

namespace DotAdr.Commands;

/// <inheritdoc cref="IConfigurationService"/>
internal class ConfigurationService(ILogger logger) : IConfigurationService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    internal string ConfigFilePath { get; } = "./dotadr.json";

    public void SaveAdrConfiguration(LocalDirectory adrDirectory, bool overwriteConfiguration)
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(SaveAdrConfiguration));

        ArgumentNullException.ThrowIfNull(adrDirectory);

        // FileInfo constructor automatically calls Path.GetFullPath() internally? So this should work across platforms...
        var fileInfo = new FileInfo(ConfigFilePath);
        if (!fileInfo.Exists || overwriteConfiguration)
        {
            var config = new DotAdrConfig(adrDirectory.NormalizedPath);
            var jsonString = JsonSerializer.Serialize(config, _jsonSerializerOptions);
            File.WriteAllText(ConfigFilePath, jsonString);
        }
        else
        {
            logger.Debug("The config file {@ConfigFile} already exists", ConfigFilePath);
        }

        logger.MethodReturn(nameof(ConfigurationService), nameof(SaveAdrConfiguration));
    }

    public DotAdrConfig GetDotAdrConfiguration()
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(GetDotAdrConfiguration));

        var fileInfo = new FileInfo(ConfigFilePath);
        if (!fileInfo.Exists)
        {
            throw new DotAdrException($"Configuration file does not exist at {ConfigFilePath}");
        }

        var jsonString = File.ReadAllText(ConfigFilePath);
        var config = JsonSerializer.Deserialize<DotAdrConfig>(jsonString, _jsonSerializerOptions);

        if (config == null)
        {
            throw new DotAdrException($"Failed to read configuration at {ConfigFilePath}");
        }

        if (string.IsNullOrWhiteSpace(config.Directory))
        {
            throw new DotAdrException($"ADR configuration directory value at {ConfigFilePath} is null or empty");
        }

        logger.MethodReturn(nameof(ConfigurationService), nameof(GetDotAdrConfiguration));

        return config;
    }
}
