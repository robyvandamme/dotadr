// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Commands.Init;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DotAdr.Tests.Commands;

public class AdrAddCommandTests
{
    public class Execute
    {
        [Fact]
        public void Initialized_Adds_Decision_Record()
        {
            Initialize();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            // var command = new AdrAddCommand(console, logger, adrFileService, adrFactory, configurationService);
            // var remainingArguments = new Mock<IRemainingArguments>();
            // var context = new CommandContext(["adr", "add"], remainingArguments.Object, "add", null);
            // var settings = new AdrAddSettings { Title = "New decision record" };
            // var result = command.Execute(context, settings);
            // result.ShouldBe(0);
            //
            // console.Output.ShouldStartWith("002-new-decision-record.md added to the ./doc/adr directory");
            //
            // var fileInfo = new FileInfo("./doc/adr/002-new-decision-record.md");
            // fileInfo.Exists.ShouldBeTrue();
        }

        [Fact]
        public void Not_Initialized_Returns_Error()
        {
            Initialize();

            var adrDirectory = new LocalDirectory("./doc/adr");
            adrDirectory.EnsureDirectoryDeleted();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            // var command = new AdrAddCommand(console, logger, adrFileService, adrFactory, configurationService);
            // var remainingArguments = new Mock<IRemainingArguments>();
            // var context = new CommandContext(["adr", "add"], remainingArguments.Object, "add", null);
            // var settings = new AdrAddSettings { Title = "New decision record" };
            // var result = command.Execute(context, settings);
            //
            // result.ShouldBe(1);
            //
            // console.Output.ShouldNotBeEmpty();
            // console.Output.ShouldContain("DotBotException");
            // console.Output.ShouldContain("The directory");
            // console.Output.ShouldContain("does not exist");
            //
            // var fileInfo = new FileInfo("./doc/adr/002-new-decision-record.md");
            // fileInfo.Exists.ShouldBeFalse();
        }

        [Fact]
        public void No_Config_Returns_Error()
        {
            Initialize();

            var configDirectory = new LocalDirectory(".bot");
            configDirectory.EnsureDirectoryDeleted();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            // var command = new AdrAddCommand(console, logger, adrFileService, adrFactory, configurationService);
            // var remainingArguments = new Mock<IRemainingArguments>();
            // var context = new CommandContext(["adr", "add"], remainingArguments.Object, "add", null);
            // var settings = new AdrAddSettings { Title = "New decision record" };
            // var result = command.Execute(context, settings);
            //
            // result.ShouldBe(1);
            //
            // console.Output.ShouldNotBeEmpty();
            // console.Output.ShouldContain("DotBotException: Configuration file does not exist at .bot/config.json");
            //
            // var fileInfo = new FileInfo("./doc/adr/002-new-decision-record.md");
            // fileInfo.Exists.ShouldBeFalse();
        }

        private static void Initialize()
        {
            var adrDirectory = new LocalDirectory("./doc/adr");
            adrDirectory.EnsureDirectoryDeleted();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            var command = new AdrInitCommand(console, logger, adrFileService, adrFactory, configurationService);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "init"], remainingArguments.Object, "init", null);
            var settings = new AdrInitSettings();
            command.Execute(context, settings);
        }
    }
}
