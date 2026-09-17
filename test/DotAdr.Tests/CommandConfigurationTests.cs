// Copyright © 2025 Roby Van Damme.

using Moq;
using Serilog;
using Shouldly;
using Spectre.Console.Cli;

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

            Should.NotThrow(() => commandApp.Configure(logger));
        }

        [Fact]
        public void Throws_When_CommandApp_Is_Null()
        {
            var logger = new Mock<ILogger>().Object;

            Should.Throw<ArgumentNullException>(() => CommandConfiguration.Configure(null!, logger));
        }

        [Fact]
        public void Throws_When_Logger_Is_Null()
        {
            var commandApp = new CommandApp();

            Should.Throw<ArgumentNullException>(() => commandApp.Configure(null!));
        }
    }
}
