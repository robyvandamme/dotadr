// Copyright © 2025 Roby Van Damme.

using System.Diagnostics;
using System.Reflection;

namespace DotAdr.Common;

internal record VersionInfo
{
    private const string UnavailableVersion = "not available";

    public VersionInfo(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var version = assembly.GetName().Version;
        if (version != null)
        {
            AssemblyVersion = version.ToString();
        }

        var location = assembly.Location;
        if (!string.IsNullOrEmpty(location) && File.Exists(location))
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(location);
            AssemblyFileVersionInfo = fileVersionInfo.FileVersion;
            ProductVersion = fileVersionInfo.ProductVersion;
        }

        Version = ResolveVersion(ProductVersion, AssemblyFileVersionInfo, AssemblyVersion);
    }

    internal static string? ParseProductVersion(string? productVersion)
    {
        if (string.IsNullOrEmpty(productVersion))
        {
            return null;
        }

        var plusSign = productVersion.IndexOf('+', StringComparison.OrdinalIgnoreCase);
        return plusSign >= 0 ? productVersion[..plusSign] : productVersion;
    }

    internal static string ResolveVersion(
        string? productVersion,
        string? fileVersion,
        string? assemblyVersion)
    {
        var parsedProductVersion = ParseProductVersion(productVersion);
        if (!string.IsNullOrWhiteSpace(parsedProductVersion))
        {
            return parsedProductVersion;
        }

        if (!string.IsNullOrWhiteSpace(fileVersion))
        {
            return fileVersion;
        }

        if (!string.IsNullOrWhiteSpace(assemblyVersion))
        {
            return assemblyVersion;
        }

        return UnavailableVersion;
    }

    public string? AssemblyVersion { get; }

    public string? AssemblyFileVersionInfo { get; }

    public string? ProductVersion { get; }

    public string Version { get; }
}
