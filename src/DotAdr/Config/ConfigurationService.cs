// Copyright © 2025 Roby Van Damme.

using System.Text.Json;
using DotAdr.Common;
using Serilog;

namespace DotAdr.Config;

public class ConfigurationService(ILogger logger) : IConfigurationService
{
    private readonly string _configFilePath = "./dotadr.json";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Save the relative ADR directory path to the configuration file.
    /// </summary>
    /// <param name="adrDirectory">The relative path of the ADR directory.</param>
    public void SaveAdrConfiguration(LocalDirectory adrDirectory)
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(SaveAdrConfiguration));

        ArgumentNullException.ThrowIfNull(adrDirectory);

        var fileInfo = new FileInfo(_configFilePath);
        if (!fileInfo.Exists)
        {
            var config = new DotAdrConfig(1, adrDirectory.RelativePath);
            SaveConfiguration(config);
        }
        else
        {
            var config = GetConfiguration();
            var newConfig = config with { Directory = adrDirectory.RelativePath };
            SaveConfiguration(newConfig);
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
            throw new DotAdrException($"No configuration found at {_configFilePath}");
        }

        if (string.IsNullOrWhiteSpace(config.Directory))
        {
            throw new DotAdrException($"ADR configuration directory value at {_configFilePath} is null or empty");
        }

        logger.MethodReturn(nameof(ConfigurationService), nameof(GetDotAdrConfiguration));

        return config;
    }

    private void SaveConfiguration(DotAdrConfig config)
    {
        logger.MethodStart(nameof(ConfigurationService), nameof(SaveConfiguration));
        ArgumentNullException.ThrowIfNull(config);
        var jsonString = JsonSerializer.Serialize(config, _jsonSerializerOptions);
        File.WriteAllText(_configFilePath, jsonString);
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
        var fileInfo = new FileInfo(_configFilePath);
        if (!fileInfo.Exists)
        {
            throw new DotAdrException($"Configuration file does not exist at {_configFilePath}");
        }

        var jsonString = File.ReadAllText(_configFilePath);
        var config = JsonSerializer.Deserialize<DotAdrConfig>(jsonString, _jsonSerializerOptions);

        if (config == null)
        {
            throw new DotAdrException($"Failed to read configuration at {_configFilePath}");
        }

        logger.MethodReturn(nameof(ConfigurationService), nameof(GetConfiguration));
        return config;
    }
}
