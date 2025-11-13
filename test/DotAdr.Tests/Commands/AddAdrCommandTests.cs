// Copyright © 2025 Roby Van Damme.

using System.Globalization;
using DotAdr.Commands;
using DotAdr.Commands.Add;
using DotAdr.Commands.Init;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DotAdr.Tests.Commands;

public class AddAdrCommandTests
{
    public class Execute
    {
        public Execute()
        {
            var adrDirectory = new LocalDirectory("./doc/adr");
            adrDirectory.EnsureDirectoryDeleted();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            var command = new InitAdrCommand(console, logger, adrFileService, adrFactory, configurationService);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "init"], remainingArguments.Object, "init", null);
            var settings = new InitAdrSettings { Overwrite = true };
            command.Execute(context, settings, CancellationToken.None);
        }

        [Fact]
        public void Adds_New_Decision_Record()
        {
            var adrDirectory = new LocalDirectory("./doc/adr");
            var fileCount = Directory.EnumerateFiles(adrDirectory.AbsolutePath).Count();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            var nextId = adrFileService.GetNextRecordId(adrDirectory);

            var command = new AddAdrCommand(console, logger, adrFileService, adrFactory, configurationService);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "add"], remainingArguments.Object, "add", null);
            var settings = new AddAdrSettings { Title = "New Decision Record" };
            var result = command.Execute(context, settings, CancellationToken.None);

            result.ShouldBe(0);
            console.Output.ShouldContain("new-decision-record.md added to the ./doc/adr directory");

            var newFileCount = Directory.EnumerateFiles(adrDirectory.AbsolutePath).Count();
            newFileCount.ShouldBe(fileCount + 1);

            var newNextId = adrFileService.GetNextRecordId(adrDirectory);

            int.Parse(newNextId, CultureInfo.InvariantCulture)
                .ShouldBe(int.Parse(nextId, CultureInfo.InvariantCulture) + 1);

            var secondRecord = new FileInfo(Path.Combine(adrDirectory.AbsolutePath, "002-new-decision-record.md"));
            secondRecord.Exists.ShouldBe(true);
            var secondRecordContent = File.ReadAllText(secondRecord.FullName);
            secondRecordContent.ShouldNotContain("* Supersedes:");
            secondRecordContent.ShouldNotContain("{{SUPERSEDES}}");
        }

        [Fact]
        public void Adds_New_Decision_Record_With_Superseding_Option()
        {
            var adrDirectory = new LocalDirectory("./doc/adr");
            var fileCount = Directory.EnumerateFiles(adrDirectory.AbsolutePath).Count();

            using var console = new TestConsole();
            console.EmitAnsiSequences = false;
            var logger = new Mock<ILogger>().Object;
            var adrFileService = new AdrFileService(logger);
            var adrFactory = new AdrFactory(logger);
            var configurationService = new ConfigurationService(logger);

            // Get the ID and check if we have added a file
            var nextId = adrFileService.GetNextRecordId(adrDirectory);

            var command = new AddAdrCommand(console, logger, adrFileService, adrFactory, configurationService);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "add"], remainingArguments.Object, "add", null);
            var settings = new AddAdrSettings { Title = "New Decision Record", Supersedes = "001" };
            var result = command.Execute(context, settings, CancellationToken.None);

            result.ShouldBe(0);
            console.Output.ShouldContain("new-decision-record.md added to the ./doc/adr directory");

            var newFileCount = Directory.EnumerateFiles(adrDirectory.AbsolutePath).Count();
            newFileCount.ShouldBe(fileCount + 1);

            var newNextId = adrFileService.GetNextRecordId(adrDirectory);

            int.Parse(newNextId, CultureInfo.InvariantCulture)
                .ShouldBe(int.Parse(nextId, CultureInfo.InvariantCulture) + 1);

            var firstRecord = new FileInfo(
                Path.Combine(adrDirectory.AbsolutePath, "001-use-architectural-decision-records.md"));
            firstRecord.Exists.ShouldBe(true);
            var firstRecordContent = File.ReadAllText(firstRecord.FullName);
            firstRecordContent.ShouldContain(
                $" - Superseded by [002](002-new-decision-record.md) {DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            var secondRecord = new FileInfo(Path.Combine(adrDirectory.AbsolutePath, "002-new-decision-record.md"));
            secondRecord.Exists.ShouldBe(true);
            var secondRecordContent = File.ReadAllText(secondRecord.FullName);
            secondRecordContent.ShouldContain("* Supersedes: [001](001-use-architectural-decision-records.md)");
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

            configurationService.Setup(c => c.GetDotAdrConfiguration()).Throws<DotAdrException>();

            var command = new AddAdrCommand(console, logger, adrFileService, adrFactory, configurationService.Object);
            var remainingArguments = new Mock<IRemainingArguments>();
            var context = new CommandContext(["adr", "add"], remainingArguments.Object, "add", null);
            var settings = new AddAdrSettings { Title = "New Decision Record" };
            var result = command.Execute(context, settings, CancellationToken.None);
            result.ShouldBe(1);
            console.Output.ShouldContain("DotAdrException");
        }
    }
}
