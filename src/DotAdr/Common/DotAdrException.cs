// Copyright © 2025 Roby Van Damme.

namespace DotAdr.Common;

internal sealed class DotAdrException : Exception
{
    public DotAdrException(string message)
        : base(message)
    {
    }

    public DotAdrException()
    {
    }

    public DotAdrException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
