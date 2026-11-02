// Copyright © Roby Van Damme.

namespace DotAdr;

internal static class ArgumentHandler
{
    private const string DebugOption = "--debug";
    private const string DebugOptionPrefix = "--debug=";
    private const string LogFileOption = "--logfile";
    private const string LogFileOptionPrefix = "--logfile=";
    private const string TrueValue = "true";
    private const string FalseValue = "false";

    /// <summary>
    /// Determines whether debug logging is enabled. Aligned with the Spectre.Console.Cli grammar so that the outcome
    /// depends only on argument forms that the application would also accept: explicit "true"/"false" values and the
    /// bare flag. An inline value other than "true" is either explicitly disabled or rejected by Spectre, so the helper
    /// fails closed (returns false) for those.
    /// </summary>
    /// <remarks>
    /// A token following a bare flag is only treated as an explicit value when it is "true" or "false". Any other
    /// token is left for Spectre to interpret (a positional argument on commands that declare one, or a parse error
    /// otherwise), so the flag is treated as bare.
    /// </remarks>
    internal static bool IsDebugMode(string[] args)
    {
        // Check if args is empty
        if (args.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(DebugOption, StringComparison.OrdinalIgnoreCase))
            {
                // An explicit "true"/"false" value token enables or disables debug logging.
                if (i < args.Length - 1 && args[i + 1].Equals(FalseValue, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (i < args.Length - 1 && IsTrue(args[i + 1]))
                {
                    return true;
                }

                // Any other token is either absent or interpreted by Spectre as a positional argument, so the flag was
                // used bare.
                return true;
            }

            // The "--debug=value" form carries its value in the same token. Only "true" enables; anything else is
            // either explicitly disabled or a value Spectre rejects.
            if (args[i].StartsWith(DebugOptionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return IsTrue(args[i][DebugOptionPrefix.Length..]);
            }
        }

        // "--debug" not found in arguments
        return false;
    }

    /// <summary>
    /// Determines the log file, if requested. Note that "--logfile=" (empty inline value) is rejected by Spectre, so
    /// it can only affect the logging configured for a run that is about to fail; returning no file name is the safe
    /// outcome.
    /// </summary>
    internal static string LogFile(string[] args)
    {
        // Check if args is empty
        if (args.Length == 0)
        {
            return string.Empty;
        }

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(LogFileOption, StringComparison.OrdinalIgnoreCase))
            {
                // Check if this is the last argument
                if (i == args.Length - 1)
                {
                    // "--logfile" is the last argument with no value
                    return string.Empty;
                }

                // A token that starts with '-' is an option (or a parse error) for Spectre, not a value.
                return IsValueToken(args[i + 1]) ? args[i + 1] : string.Empty;
            }

            // The "--logfile=value" form carries its value in the same token.
            if (args[i].StartsWith(LogFileOptionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return args[i][LogFileOptionPrefix.Length..];
            }
        }

        // "--logfile" not found in arguments
        return string.Empty;
    }

    private static bool IsValueToken(string token)
    {
        // Spectre tokenizes any argument that starts with '-' as an option (or a parse error), except tokens where
        // the first character after the dash is a digit (for example "-1" or "-1.log"). Empty arguments are values.
        return token.Length == 0
            || token[0] != '-'
            || (token.Length > 1 && char.IsDigit(token[1]));
    }

    private static bool IsTrue(string value)
    {
        return value.Equals(TrueValue, StringComparison.OrdinalIgnoreCase);
    }
}
