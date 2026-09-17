// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;

namespace DotAdr.Tests.Commands;

public class AdrFileServiceTests
{
    public class AddDecisionRecord
    {
        [Fact]
        public void Throws_When_Directory_Does_Not_Exist()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);
            var record = new DecisionRecord("001", "A decision", "content");

            directory.EnsureDirectoryDeleted();

            Should.Throw<DotAdrException>(() => service.AddDecisionRecord(directory, record));
        }
    }

    public class InitializeDirectory
    {
        [Fact]
        public void Does_Not_Overwrite_Existing_Files_When_Overwrite_Is_False()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);

            try
            {
                directory.EnsureDirectoryCreated();
                directory.EnsureFileCreated("template.md", "Original template");
                directory.EnsureFileCreated("001-initial-decision.md", "Original decision");

                service.InitializeDirectory(
                    directory,
                    "Replacement template",
                    new DecisionRecord("001", "Initial decision", "Replacement decision"),
                    false);

                File.ReadAllText(Path.Combine(directory.AbsolutePath, "template.md"))
                    .ShouldBe("Original template");
                File.ReadAllText(Path.Combine(directory.AbsolutePath, "001-initial-decision.md"))
                    .ShouldBe("Original decision");
            }
            finally
            {
                directory.EnsureDirectoryDeleted();
            }
        }

        [Fact]
        public void Overwrites_Existing_Files_When_Overwrite_Is_True()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);

            try
            {
                directory.EnsureDirectoryCreated();
                directory.EnsureFileCreated("template.md", "Original template");
                directory.EnsureFileCreated("001-initial-decision.md", "Original decision");

                service.InitializeDirectory(
                    directory,
                    "Replacement template",
                    new DecisionRecord("001", "Initial decision", "Replacement decision"),
                    true);

                File.ReadAllText(Path.Combine(directory.AbsolutePath, "template.md"))
                    .ShouldBe("Replacement template");
                File.ReadAllText(Path.Combine(directory.AbsolutePath, "001-initial-decision.md"))
                    .ShouldBe("Replacement decision");
            }
            finally
            {
                directory.EnsureDirectoryDeleted();
            }
        }
    }

    public class GetNextRecordId
    {
        [Fact]
        public void Returns_001_When_Directory_Contains_No_Record_Files()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);

            try
            {
                directory.EnsureDirectoryCreated();

                service.GetNextRecordId(directory).ShouldBe("001");
            }
            finally
            {
                directory.EnsureDirectoryDeleted();
            }
        }

        [Fact]
        public void Ignores_Files_That_Do_Not_Match_Record_Naming_Convention()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);

            try
            {
                directory.EnsureDirectoryCreated();
                File.WriteAllText(Path.Combine(directory.AbsolutePath, "002-second.md"), "second");
                File.WriteAllText(Path.Combine(directory.AbsolutePath, "010-tenth.md"), "tenth");
                File.WriteAllText(Path.Combine(directory.AbsolutePath, "12-invalid.md"), "invalid");
                File.WriteAllText(Path.Combine(directory.AbsolutePath, "abc-invalid.md"), "invalid");
                File.WriteAllText(Path.Combine(directory.AbsolutePath, "011-invalid.txt"), "invalid");

                service.GetNextRecordId(directory).ShouldBe("011");
            }
            finally
            {
                directory.EnsureDirectoryDeleted();
            }
        }

        [Fact]
        public void Increments_Record_Id_Beyond_099()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);

            try
            {
                directory.EnsureDirectoryCreated();
                File.WriteAllText(Path.Combine(directory.AbsolutePath, "099-last-record.md"), "last");

                service.GetNextRecordId(directory).ShouldBe("100");
            }
            finally
            {
                directory.EnsureDirectoryDeleted();
            }
        }
    }

    public class TryFindSupersededDecisionRecord
    {
        [Fact]
        public void Throws_When_Record_Cannot_Be_Found()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);

            try
            {
                directory.EnsureDirectoryCreated();

                Should.Throw<DotAdrException>(() => service.TryFindSupersededDecisionRecord("001", directory));
            }
            finally
            {
                directory.EnsureDirectoryDeleted();
            }
        }
    }

    public class SaveSupersedeDecisionRecord
    {
        [Fact]
        public void Throws_When_Superseded_Record_File_Does_Not_Exist()
        {
            var directory = CreateTemporaryDirectory();
            var service = new AdrFileService(new Mock<ILogger>().Object);
            var record = new SupersededDecisionRecord("001", "001-old-decision.md", "old content");

            try
            {
                directory.EnsureDirectoryCreated();

                Should.Throw<DotAdrException>(() =>
                    service.SaveSupersedeDecisionRecord(directory, record, "updated content"));
            }
            finally
            {
                directory.EnsureDirectoryDeleted();
            }
        }
    }

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

        [Fact]
        public void Returns_Custom_Template_When_Path_Is_Provided()
        {
            var directory = new LocalDirectory("adr");
            directory.EnsureDirectoryDeleted();

            var service = new AdrFileService(new Mock<ILogger>().Object);
            service.InitializeDirectory(
                directory,
                "Default Template Content",
                new DecisionRecord("001", "title", "no content"),
                false);

            var customTemplateFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "custom-template.md");
            Directory.CreateDirectory(Path.GetDirectoryName(customTemplateFile)!);
            File.WriteAllText(customTemplateFile, "Custom Template Content");

            try
            {
                var template = service.GetTemplate(directory, customTemplateFile);
                template.ShouldBe("Custom Template Content");
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(customTemplateFile)!, true);
            }
        }

        [Fact]
        public void Throws_When_Custom_Template_Does_Not_Exist()
        {
            var directory = new LocalDirectory("adr");
            directory.EnsureDirectoryDeleted();

            var service = new AdrFileService(new Mock<ILogger>().Object);
            Should.Throw<DotAdrException>(() => service.GetTemplate(directory, "non-existent-template.md"));
        }
    }

    private static LocalDirectory CreateTemporaryDirectory()
    {
        return new LocalDirectory(Path.Combine(Path.GetTempPath(), "dotadr-tests", Guid.NewGuid().ToString("N")));
    }
}
