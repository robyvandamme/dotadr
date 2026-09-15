// Copyright © 2025 Roby Van Damme.

using System.Globalization;
using DotAdr.Commands;
using Moq;
using Serilog;
using Shouldly;

namespace DotAdr.Tests.Commands;

public class AdrFactoryTests
{
    public class CreateDecisionRecord
    {
        [Fact]
        public void Replaces_Template_Variables()
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);

            var template = factory.CreateDecisionTemplate();

            var record = factory.CreateDecisionRecord(template, "005", "Decision Title", null);

            record.Id.ShouldBe("005");
            record.Title.ShouldBe("Decision Title");
            record.Content.ShouldNotBeEmpty();
            record.Content.ShouldContain("005");
            record.Content.ShouldContain("Decision Title");
            record.Content.ShouldContain(
                DateOnly.FromDateTime(DateTime.Today)
                    .ToString("O", CultureInfo.InvariantCulture));
            record.Content.ShouldNotContain("* Supersedes:");
            record.Content.ShouldNotContain("{{SUPERSEDES}}");
        }

        [Theory]
        [InlineData("* Status: {{STATUS}}")]
        [InlineData("* Author: {{AUTHOR}}")]
        public void Keeps_Lines_With_Unused_Template_Placeholders(string templateLine)
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);

            var record = factory.CreateDecisionRecord(templateLine, "005", "Decision Title", null);

            record.Content.ShouldContain(templateLine);
        }

        [Fact]
        public void Keeps_Literal_Supersedes_Line()
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);

            const string Template = "* Supersedes:";

            var record = factory.CreateDecisionRecord(Template, "005", "Decision Title", null);

            record.Content.ShouldBe(Template);
        }

        [Theory]
        [InlineData("Supersedes: {{SUPERSEDES}}")]
        [InlineData("Supersedes: {{SUPERSEDES}} with trailing text")]
        public void Removes_Custom_Supersedes_Lines_When_Record_Is_Not_Supplied(string supersedesLine)
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);

            var template = $"Before\n{supersedesLine}\nAfter";

            var record = factory.CreateDecisionRecord(template, "005", "Decision Title", null);

            record.Content.ShouldBe("Before\nAfter");
        }

        [Fact]
        public void Replaces_Supersedes_Template_Variable_When_Record_Is_Supplied()
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);
            var supersededRecord = new SupersededDecisionRecord("001", "001-old-decision.md", "content");

            var record = factory.CreateDecisionRecord(
                "* Supersedes: {{SUPERSEDES}}",
                "005",
                "Decision Title",
                supersededRecord);

            record.Content.ShouldBe("* Supersedes: [001](001-old-decision.md)");
        }
    }

    public class UpdateSupersededDecisionContent
    {
        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("\r")]
        public void Preserves_Line_Endings(string newline)
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);
            var supersededRecord = new SupersededDecisionRecord(
                "001",
                "001-old-decision.md",
                $"# Decision{newline}{newline}* Status: Accepted{newline}{newline}## Context");
            var supersedingRecord = new DecisionRecord("002", "New decision", "content");

            var result = factory.UpdateSupersededDecisionContent(
                supersededRecord,
                supersedingRecord,
                "002-new-decision.md");

            result.ShouldBe(
                $"# Decision{newline}{newline}" +
                $"* Status: Accepted - Superseded by [002](002-new-decision.md) " +
                $"{DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}{newline}{newline}" +
                "## Context");
        }

        [Fact]
        public void Preserves_Mixed_Line_Endings()
        {
            var logger = new Mock<ILogger>().Object;
            var factory = new AdrFactory(logger);
            var supersededRecord = new SupersededDecisionRecord(
                "001",
                "001-old-decision.md",
                "# Decision\r\n\r\n* Status: Accepted\n\n## Context\r");
            var supersedingRecord = new DecisionRecord("002", "New decision", "content");

            var result = factory.UpdateSupersededDecisionContent(
                supersededRecord,
                supersedingRecord,
                "002-new-decision.md");

            result.ShouldBe(
                "# Decision\r\n\r\n" +
                $"* Status: Accepted - Superseded by [002](002-new-decision.md) " +
                $"{DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}\n\n## Context\r");
        }
    }
}
