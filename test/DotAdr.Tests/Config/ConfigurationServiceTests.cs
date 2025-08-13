// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;
using DotAdr.Config;
using Moq;
using Serilog;
using Shouldly;

namespace DotAdr.Tests.Config;

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

            service.SaveAdrConfiguration(directory);

            var config = service.GetDotAdrConfiguration();
            config.Directory.ShouldBe(directory.RelativePath);
        }

        [Fact]
        public void Throws_When_Configuration_File_Already_Exists()
        {
            var service = new ConfigurationService(new Mock<ILogger>().Object);
            var directory = new LocalDirectory("test/adr");

            service.ConfigFilePath.EnsureFileDeleted();

            service.SaveAdrConfiguration(directory);

            Should.Throw<DotAdrException>(() => service.SaveAdrConfiguration(directory));
        }
    }
}
