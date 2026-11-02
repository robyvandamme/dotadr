// Copyright © Roby Van Damme.

using Serilog;
using Serilog.Events;

namespace DotAdr.Common;

/// <summary>
/// An <see cref="ILogger"/> that forwards to the current <see cref="Log.Logger"/>.
/// Logging is configured from the bound command settings by the <see cref="DotAdr.LoggingInterceptor"/>, which runs
/// after dependency injection has registered the logger, so the logger instance handed to commands must resolve the
/// active logger at call time.
/// </summary>
/// <remarks>
/// Only <see cref="Write(LogEvent)"/> and <see cref="IsEnabled"/> are forwarded. The other <see cref="ILogger"/>
/// members, including the convenience <c>Write</c> overloads and <c>ForContext</c>, are Serilog 4 default interface
/// implementations that funnel into <see cref="Write(LogEvent)"/>; the small surface is intentional.
/// </remarks>
internal sealed class DelegatingLogger : ILogger
{
    /// <inheritdoc/>
    public bool IsEnabled(LogEventLevel level)
    {
        return Log.Logger.IsEnabled(level);
    }

    /// <inheritdoc/>
    public void Write(LogEvent logEvent)
    {
        Log.Logger.Write(logEvent);
    }
}
