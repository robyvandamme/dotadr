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
/// The remaining <see cref="ILogger"/> members are provided by Serilog 4's default interface implementations, which
/// funnel into the forwarded members above; the small surface is intentional.
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

    /// <inheritdoc/>
    public void Write(
        LogEventLevel level,
        Exception? exception,
        string messageTemplate,
        params object?[]? propertyValues)
    {
        Log.Logger.Write(level, exception, messageTemplate, propertyValues);
    }
}
