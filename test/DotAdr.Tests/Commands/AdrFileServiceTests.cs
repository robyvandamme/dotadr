// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;

namespace DotAdr.Tests.Commands;

public class AdrFileServiceTests
{
    public class GetTemplate
    {
        [Fact]
        public void Throws_When_Directory_Does_Not_Exist()
        {
            var service = new AdrFileService(new Mock<ILogger>().Object);
            var directory = new LocalDirectory("no-directory-here");
            directory.EnsureDirectoryDeleted();
            Should.Throw<DotAdrException>(() => service.GetTemplate(directory));
        }

        [Fact]
        public void Throws_When_File_Does_Not_Exist()
        {
            var service = new AdrFileService(new Mock<ILogger>().Object);
            var directory = new LocalDirectory("./test/adr");
            directory.EnsureDirectoryCreated();

            // verify, just to be sure
            var info = new DirectoryInfo(directory.AbsolutePath);
            info.Exists.ShouldBe(true);

            Should.Throw<DotAdrException>(() => service.GetTemplate(directory));
        }

        [Fact]
        public void Returns_Template_When_Initialized()
        {
            var directory = new LocalDirectory("adr");
            directory.EnsureDirectoryDeleted();

            var service = new AdrFileService(new Mock<ILogger>().Object);
            service.InitializeDirectory(
                directory,
                "Template Content",
                new DecisionRecord("001", "title", "no content"),
                false);

            var template = service.GetTemplate(directory);
            template.ShouldNotBeEmpty();
            template.ShouldContain("Template Content");
        }
    }
}
