// Copyright © 2025 Roby Van Damme.

namespace DotAdr.Common;

internal static class PlatformInfo
{
    internal static string GetPlatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return "Windows";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "macOS";
        }

        if (OperatingSystem.IsLinux())
        {
            return "Linux";
        }

        if (OperatingSystem.IsFreeBSD())
        {
            return "FreeBSD";
        }

        return "Unknown OS";
    }
}
