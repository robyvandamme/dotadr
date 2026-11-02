// Copyright © Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Commands.Init;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;
using Spectre.Console;
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

            var recordContent = File.ReadAllText(initialDecisionRecord.FullName);
            recordContent.ShouldContain("# 001 Use Architectural Decision Records");
            recordContent.ShouldContain("## Consequences");
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
                Directory = adrDirectory.RelativePath,
                TemplatePath = customTemplateFile.FullName,
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

        [Fact]
        public void Returns_Failure_When_Command_Name_Is_Unsupported()
        {
            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new Mock<IAdrFileService>();
            var adrFactory = new Mock<IAdrFactory>();
            var configurationService = new Mock<IConfigurationService>();

            var command = new InitAdrCommand(
                console,
                logger,
                adrFileService.Object,
                adrFactory.Object,
                configurationService.Object);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "unsupported"], remainingArguments.Object, "unsupported", null);
            var result = command.Execute(context, new InitAdrSettings(), CancellationToken.None);

            result.ShouldBe(1);
            console.Output.ShouldContain("Unsupported command name unsupported");
            configurationService.Verify(
                service => service.SaveAdrConfiguration(It.IsAny<LocalDirectory>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public void Returns_Success_When_Directory_Path_Contains_Square_Brackets()
        {
            var adrDirectory = new LocalDirectory(
                Path.Combine(Path.GetTempPath(), $"dotadr-[adr]-{Guid.NewGuid():N}"));
            var configurationService = new Mock<IConfigurationService>();

            try
            {
                using var console = new TestConsole();
                console.EmitAnsiSequences = false;
                var logger = new Mock<ILogger>().Object;
                var adrFileService = new AdrFileService(logger);
                var adrFactory = new AdrFactory(logger);

                var command = new InitAdrCommand(
                    console,
                    logger,
                    adrFileService,
                    adrFactory,
                    configurationService.Object);
                var remainingArguments = new Mock<IRemainingArguments>();
                var context = new CommandContext(["adr", "init"], remainingArguments.Object, "init", null);
                var settings = new InitAdrSettings { Directory = adrDirectory.RelativePath, Overwrite = true };

                var result = command.Execute(context, settings, CancellationToken.None);

                File.Exists(Path.Combine(adrDirectory.AbsolutePath, "template.md")).ShouldBeTrue();
                File.Exists(
                        Path.Combine(adrDirectory.AbsolutePath, "001-use-architectural-decision-records.md"))
                    .ShouldBeTrue();
                result.ShouldBe(0);
            }
            finally
            {
                adrDirectory.EnsureDirectoryDeleted();
            }
        }
    }

    public class RunAsync
    {
        [Theory]
        [InlineData("--debug")]
        [InlineData("--debug=true")]
        [InlineData("--debug=false")]
        [InlineData("--logfile=app.log")]
        [InlineData("--debug", "--logfile", "app.log")]
        public async Task With_Debug_And_LogFile_Syntax_Accepted_By_Spectre_Returns_0(params string[] arguments)
        {
            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var configurationService = new Mock<IConfigurationService>();
            var app = CreateApp(console, configurationService.Object);

            var result = await app.RunAsync(["init", .. arguments]);

            result.ShouldBe(0);
        }

        [Theory]
        [InlineData("-o")]
        [InlineData("-o", "true")]
        public async Task With_Overwrite_Syntax_Sets_Overwrite_To_True(params string[] arguments)
        {
            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var configurationService = new Mock<IConfigurationService>();
            var app = CreateApp(console, configurationService.Object);

            var result = await app.RunAsync(["init", .. arguments]);

            result.ShouldBe(0);
            configurationService.Verify(
                service => service.SaveAdrConfiguration(It.IsAny<LocalDirectory>(), true),
                Times.Once);
        }

        [Fact]
        public async Task With_Explicitly_Disabled_Overwrite_Sets_Overwrite_To_False()
        {
            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var configurationService = new Mock<IConfigurationService>();
            var app = CreateApp(console, configurationService.Object);

            var result = await app.RunAsync(["init", "-o", "false"]);

            result.ShouldBe(0);
            configurationService.Verify(
                service => service.SaveAdrConfiguration(It.IsAny<LocalDirectory>(), false),
                Times.Once);
        }

        private static CommandApp CreateApp(TestConsole console, IConfigurationService configurationService)
        {
            IAnsiConsole ansiConsole = console;
            var app = new CommandApp();

            app.Configure(config =>
            {
                config.PropagateExceptions();

                // Set the console on the settings as well: Spectre caches the default console taken from
                // AnsiConsole.Console statically, which would otherwise leak a disposed TestConsole between tests
                // that swap the global console.
                config.Settings.Console = ansiConsole;
                config.Settings.Registrar.RegisterInstance(ansiConsole);
                config.Settings.Registrar.RegisterInstance(new Mock<ILogger>().Object);
                config.Settings.Registrar.RegisterInstance(new Mock<IAdrFileService>().Object);
                config.Settings.Registrar.RegisterInstance(new Mock<IAdrFactory>().Object);
                config.Settings.Registrar.RegisterInstance(configurationService);
                config.AddCommand<InitAdrCommand>("init");
            });

            return app;
        }
    }
}
