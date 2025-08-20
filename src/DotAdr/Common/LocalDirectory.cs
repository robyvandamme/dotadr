// Copyright © 2025 Roby Van Damme.

namespace DotAdr.Common;

internal record LocalDirectory(string RelativePath)
{
    public string AbsolutePath { get; } = Path.GetFullPath(RelativePath);

    public string NormalizedPath { get; } = RelativePath.Replace('\\', '/');

    public bool Exists()
    {
        return Directory.Exists(AbsolutePath);
    }
}
