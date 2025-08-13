// Copyright © 2025 Roby Van Damme.

using System.Text.Json;
using DotAdr.Common;
using Serilog;

namespace DotAdr.Config;

internal class ConfigurationService(ILogger logger) : IConfigurationService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    internal string ConfigFilePath { get; } = "./dotadr.json";

    /// <summary>
    /// Save the relative ADR directory path to the configuration file.
    /// </summary>
    /// <param name="adrDirectory">The relative path of the ADR directory.</param>
    public void SaveAdrConfiguration(LocalDirectory adrDirectory)
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(SaveAdrConfiguration));

        ArgumentNullException.ThrowIfNull(adrDirectory);

        var fileInfo = new FileInfo(ConfigFilePath);
        if (!fileInfo.Exists)
        {
            var config = new DotAdrConfig(adrDirectory.RelativePath);
            SaveConfiguration(config);
        }
        else
        {
            // There already is a dotadr.json configuration file. What does that mean? Init was already called.... Throw for now....
            throw new DotAdrException($"There is already a configuration file present at {ConfigFilePath}");
        }

        logger.MethodReturn(nameof(ConfigurationService), nameof(SaveAdrConfiguration));
    }

    /// <summary>
    /// Gets DotBot config from the .bot directory.
    /// </summary>
    /// <returns><see cref="DotAdrConfig"/>The configuration.</returns>
    /// <exception cref="DotAdrException">When the config can not be found or has missing elements.</exception>
    public DotAdrConfig GetDotAdrConfiguration()
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(GetDotAdrConfiguration));

        var config = GetConfiguration();
        if (config == null)
        {
            throw new DotAdrException($"No configuration found at {ConfigFilePath}");
        }

        if (string.IsNullOrWhiteSpace(config.Directory))
        {
            throw new DotAdrException($"ADR configuration directory value at {ConfigFilePath} is null or empty");
        }

        logger.MethodReturn(nameof(ConfigurationService), nameof(GetDotAdrConfiguration));

        return config;
    }

    private void SaveConfiguration(DotAdrConfig config)
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(SaveConfiguration));
        ArgumentNullException.ThrowIfNull(config);
        var jsonString = JsonSerializer.Serialize(config, _jsonSerializerOptions);
        File.WriteAllText(ConfigFilePath, jsonString);
        logger.MethodReturn(nameof(ConfigurationService), nameof(SaveConfiguration));
    }

    /// <summary>
    /// Get the configuration.
    /// </summary>
    /// <returns><see cref="DotAdrConfig"/>The configuration.</returns>
    /// <exception cref="DotAdrException">When an error occurs reading the configuration file.</exception>
    private DotAdrConfig GetConfiguration()
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(GetConfiguration));
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

        logger.MethodReturn(nameof(ConfigurationService), nameof(GetConfiguration));
        return config;
    }
}
