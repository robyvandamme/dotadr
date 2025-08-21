// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Commands.Init;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DotAdr.Tests.Commands.Init;

public class InitAdrCommandTests
{
    public class Execute
    {
        [Fact]
        public void Creates_Configuration_And_Initializes_Directory()
        {
            var adrDirectory = new LocalDirectory("./doc/adr");
            adrDirectory.EnsureDirectoryDeleted();

            var configuration = new FileInfo("./dotadr.json");

            configuration.Delete();
            configuration.Refresh();

            var adrTemplate = new FileInfo("./doc/adr/template.md");
            var initialDecisionRecord = new FileInfo("./doc/adr/001-use-architectural-decision-records.md");

            configuration.Exists.ShouldBeFalse();
            adrTemplate.Exists.ShouldBeFalse();
            initialDecisionRecord.Exists.ShouldBeFalse();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            var command = new InitAdrCommand(console, logger, adrFileService, adrFactory, configurationService);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "init"], remainingArguments.Object, "init", null);
            var settings = new InitAdrSettings();
            var result = command.Execute(context, settings);

            result.ShouldBe(0);
            console.Output.ShouldContain("ADR directory ./doc/adr initialized");

            configuration.Refresh();
            adrTemplate.Refresh();
            initialDecisionRecord.Refresh();

            configuration.Exists.ShouldBeTrue();
            adrTemplate.Exists.ShouldBeTrue();
            initialDecisionRecord.Exists.ShouldBeTrue();
        }
    }
}
