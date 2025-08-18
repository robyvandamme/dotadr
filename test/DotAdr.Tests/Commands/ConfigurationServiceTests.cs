// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands;
using DotAdr.Common;
using Moq;
using Serilog;
using Shouldly;

namespace DotAdr.Tests.Commands;

public class ConfigurationServiceTests
{
    public class SaveAdrConfiguration
    {
        [Fact]
        public void Saves_Configuration_To_File()
        {
            var service = new ConfigurationService(new Mock<ILogger>().Object);
            var directory = new LocalDirectory("test/adr");

            service.ConfigFilePath.EnsureFileDeleted();

            service.SaveAdrConfiguration(directory, false);

            var config = service.GetDotAdrConfiguration();
            config.Directory.ShouldBe(directory.RelativePath);
        }

        [Fact]
        public void Overwrites_When_Configuration_File_Already_Exists_And_Overwrite_True()
        {
            var service = new ConfigurationService(new Mock<ILogger>().Object);
            var adrDirectory = new LocalDirectory("test/adr");

            service.ConfigFilePath.EnsureFileDeleted();

            service.SaveAdrConfiguration(adrDirectory, true);

            var differentAdrDirectory = new LocalDirectory("other/adr");

            service.SaveAdrConfiguration(differentAdrDirectory, true);

            var config = service.GetDotAdrConfiguration();
            config.Directory.ShouldBe(differentAdrDirectory.RelativePath);
        }

        [Fact]
        public void Does_Not_Overwrite_When_Configuration_File_Already_Exists_And_Overwrite_False()
        {
            var service = new ConfigurationService(new Mock<ILogger>().Object);
            var adrDirectory = new LocalDirectory("test/adr");

            service.ConfigFilePath.EnsureFileDeleted();

            service.SaveAdrConfiguration(adrDirectory, true);

            var differentAdrDirectory = new LocalDirectory("other/adr");

            service.SaveAdrConfiguration(differentAdrDirectory, false);

            var config = service.GetDotAdrConfiguration();
            config.Directory.ShouldBe(adrDirectory.RelativePath);
        }
    }

    public class GetDotAdrConfiguration
    {
        [Fact]
        public void Gets_Configuration()
        {
            var service = new ConfigurationService(new Mock<ILogger>().Object);
            var directory = new LocalDirectory("test/adr");

            service.ConfigFilePath.EnsureFileDeleted();

            service.SaveAdrConfiguration(directory, false);

            var config = service.GetDotAdrConfiguration();
            config.Directory.ShouldBe(directory.RelativePath);
        }

        [Fact]
        public void Throws_When_Configuration_File_Not_Present()
        {
            var service = new ConfigurationService(new Mock<ILogger>().Object);

            service.ConfigFilePath.EnsureFileDeleted();

            Should.Throw<DotAdrException>(() => service.GetDotAdrConfiguration());
        }
    }
}
