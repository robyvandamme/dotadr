// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Commands.Add;
using DotAdr.Commands.Init;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DotAdr.Tests.Commands.Add;

public class AddAdrCommandTests
{
    public class Execute
    {
        public Execute()
        {
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
            command.Execute(context, settings);
        }

        [Fact]
        public void Adds_New_Decision_Record()
        {
            // How do we validate?
            // Do a count of the files in the directory, and if we have one more we are happy?
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
            var settings = new AddAdrSettings { Title = "New Decision Record" };
            var result = command.Execute(context, settings);

            result.ShouldBe(0);
            console.Output.ShouldContain("new-decision-record.md added to the ./doc/adr directory");

            var newFileCount = Directory.EnumerateFiles(adrDirectory.AbsolutePath).Count();
            newFileCount.ShouldBe(fileCount + 1);

            var newNextId = adrFileService.GetNextRecordId(adrDirectory);
#pragma warning disable CA1305
            int.Parse(newNextId).ShouldBe(int.Parse(nextId) + 1);
#pragma warning restore CA1305
        }
    }
}
