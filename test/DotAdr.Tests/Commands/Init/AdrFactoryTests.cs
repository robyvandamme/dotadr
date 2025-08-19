// Copyright © 2025 Roby Van Damme.

using System.Globalization;
using DotAdr.Commands;
using Moq;
using Serilog;
using Shouldly;

namespace DotAdr.Tests.Commands.Init;

public class AdrFactoryTests
{
    public class CreateDecisionTemplate
    {
        // Not really anything I want to test here? Template looks OK....
    }

    public class CreateDecisionRecord
    {
        [Fact]
        public void Replaces_Template_Variables()
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);

            var template = factory.CreateDecisionTemplate();
            var record = factory.CreateDecisionRecord(template, "005", "Decision Title");

            record.Id.ShouldBe("005");
            record.Title.ShouldBe("Decision Title");
            record.Content.ShouldNotBeEmpty();
            record.Content.ShouldContain("005");
            record.Content.ShouldContain("Decision Title");
            record.Content.ShouldContain(
                DateOnly.FromDateTime(DateTime.Today)
                    .ToString("O", CultureInfo.InvariantCulture));
        }
    }
}
