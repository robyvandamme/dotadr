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
            var result = command.Execute(context, settings, CancellationToken.None);

            result.ShouldBe(0);
            console.Output.ShouldContain("ADR directory ./doc/adr initialized");

            configuration.Refresh();
            adrTemplate.Refresh();
            initialDecisionRecord.Refresh();

            configuration.Exists.ShouldBeTrue();
            adrTemplate.Exists.ShouldBeTrue();
            initialDecisionRecord.Exists.ShouldBeTrue();
        }

        [Fact]
        public void Initializes_Directory_With_Custom_Template()
        {
            var adrDirectory = new LocalDirectory("./doc/adr-custom");
            adrDirectory.EnsureDirectoryDeleted();

            var customTemplateFile = new FileInfo("./custom-template.md");
            var customContent =
                "# {{ID}} {{TITLE}}\n\nCustom Template Content\n\n* Status: {{STATUS}}\n* Date: {{DATE}}";
            File.WriteAllText(customTemplateFile.FullName, customContent);

            var configuration = new FileInfo("./dotadr.json");
            configuration.Delete();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            var command = new InitAdrCommand(console, logger, adrFileService, adrFactory, configurationService);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "init"], remainingArguments.Object, "init", null);
            var settings = new InitAdrSettings
            {
                Directory = adrDirectory.RelativePath, TemplatePath = customTemplateFile.FullName,
            };

            var result = command.Execute(context, settings, CancellationToken.None);

            result.ShouldBe(0);

            var adrTemplate = new FileInfo(Path.Combine(adrDirectory.AbsolutePath, "template.md"));
            adrTemplate.Exists.ShouldBeTrue();
            File.ReadAllText(adrTemplate.FullName).ShouldBe(customContent);

            var initialDecisionRecord = new FileInfo(
                Path.Combine(adrDirectory.AbsolutePath, "001-use-architectural-decision-records.md"));
            initialDecisionRecord.Exists.ShouldBeTrue();
            var recordContent = File.ReadAllText(initialDecisionRecord.FullName);
            recordContent.ShouldContain("Custom Template Content");
            recordContent.ShouldContain("# 001 Use Architectural Decision Records");

            // Cleanup
            adrDirectory.EnsureDirectoryDeleted();
            customTemplateFile.Delete();
            configuration.Delete();
        }

        [Fact]
        public void Catches_Exceptions_And_Returns_Failure_Result()
        {
            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new Mock<IConfigurationService>();

            configurationService.Setup(c => c.SaveAdrConfiguration(It.IsAny<LocalDirectory>(), It.IsAny<bool>()))
                .Throws<DotAdrException>();

            var command = new InitAdrCommand(console, logger, adrFileService, adrFactory, configurationService.Object);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "init"], remainingArguments.Object, "init", null);
            var settings = new InitAdrSettings();
            var result = command.Execute(context, settings, CancellationToken.None);

            result.ShouldBe(1);
            console.Output.ShouldContain("DotAdrException");
        }
    }
}
