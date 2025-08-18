// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;

namespace DotAdr.Tests.Commands;

public class AdrFileServiceTests
{
    public class InitializeDirectory
    {
        [Fact(Skip = "Review: covered by the init command test.")]
        public void Creates_Directory_If_Not_Exists()
        {
            var directory = new LocalDirectory("adr");
            directory.EnsureDirectoryDeleted();

            var service = new AdrFileService(new Mock<ILogger>().Object);

            var factory = new AdrFactory(new Mock<ILogger>().Object);
            var template = factory.CreateDecisionTemplate();
            var record = factory.CreateDecisionRecord(1, template, "New decision record");
            service.InitializeDirectory(directory, template, record);

            var info = new DirectoryInfo(directory.AbsolutePath);
            info.Exists.ShouldBe(true);
            var templateInfo = new FileInfo(Path.Combine(directory.AbsolutePath, "template.md"));
            templateInfo.Exists.ShouldBe(true);
        }
    }
}
