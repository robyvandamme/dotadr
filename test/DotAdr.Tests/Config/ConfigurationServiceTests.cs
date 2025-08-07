// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;

namespace DotAdr.Tests.Config;

public class ConfigurationServiceTests
{
    public class SaveAdrConfiguration
    {
        [Fact]
        public void Saves_Configuration_To_File()
        {
            var configDirectory = new LocalDirectory(".bot");
            configDirectory.EnsureDirectoryDeleted();

            // var service = new ConfigurationService(new Mock<ILogger>().Object);
            // var directory = new LocalDirectory("test/adr");
            // service.SaveAdrConfiguration(directory);
            //
            // var config = service.GetDotBotConfiguration();
            // config.Adr.Directory.ShouldBe(directory.RelativePath);
        }
    }
}
