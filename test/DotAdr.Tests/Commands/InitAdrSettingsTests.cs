// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands.Init;
using Shouldly;
using Spectre.Console;

namespace DotAdr.Tests.Commands;

public class InitAdrSettingsTests
{
    [Fact]
    public void Validate_Should_Return_Success_When_TemplatePath_Exists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "template.md");
        File.WriteAllText(tempFile, "content");

        var settings = new InitAdrSettings
        {
            Directory = tempDir,
            TemplatePath = tempFile,
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.ShouldBeTrue();

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Validate_Should_Return_Error_When_TemplatePath_Does_Not_Exist()
    {
        // Arrange
        var settings = new InitAdrSettings
        {
            TemplatePath = "non-existent-file.md",
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldBe("The template path 'non-existent-file.md' does not exist.");
    }
}
