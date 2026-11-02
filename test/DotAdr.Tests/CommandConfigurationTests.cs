// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DotAdr.Tests;

public class CommandConfigurationTests
{
    public class Configure
    {
        [Fact]
        public void Registers_Commands_And_Services()
        {
            var commandApp = new CommandApp();
            var logger = new Mock<ILogger>().Object;
            var versionInfo = CreateVersionInfo();

            Should.NotThrow(() => commandApp.Configure(logger, versionInfo));
        }

        [Fact]
        public void Prints_Application_Version()
        {
            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var commandApp = new CommandApp();
            var logger = new Mock<ILogger>().Object;
            var versionInfo = CreateVersionInfo();
            var originalConsole = AnsiConsole.Console;

            try
            {
                // Spectre resolves AnsiConsole.Console at run time when no console is configured.
                AnsiConsole.Console = console;

                commandApp.Configure(logger, versionInfo);

                var result = commandApp.Run(["--version"]);

                result.ShouldBe(0);
                console.Output.ShouldContain(versionInfo.Version);
            }
            finally
            {
                AnsiConsole.Console = originalConsole;
            }
        }

        [Fact]
        public void Throws_When_CommandApp_Is_Null()
        {
            var logger = new Mock<ILogger>().Object;
            var versionInfo = CreateVersionInfo();

            Should.Throw<ArgumentNullException>(() => CommandConfiguration.Configure(null!, logger, versionInfo));
        }

        [Fact]
        public void Throws_When_Logger_Is_Null()
        {
            var commandApp = new CommandApp();
            var versionInfo = CreateVersionInfo();

            Should.Throw<ArgumentNullException>(() => commandApp.Configure(null!, versionInfo));
        }

        [Fact]
        public void Throws_When_VersionInfo_Is_Null()
        {
            var commandApp = new CommandApp();
            var logger = new Mock<ILogger>().Object;

            Should.Throw<ArgumentNullException>(() => commandApp.Configure(logger, null!));
        }

        private static VersionInfo CreateVersionInfo() => new(typeof(CommandConfiguration).Assembly);
    }
}
