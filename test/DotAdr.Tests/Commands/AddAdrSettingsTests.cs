// Copyright © 2025 Roby Van Damme.

using DotAdr.Commands.Add;
using Shouldly;

namespace DotAdr.Tests.Commands;

public class AddAdrSettingsTests
{
    [Fact]
    public void Validate_Should_Return_Success_When_TemplatePath_Exists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "template.md");
        File.WriteAllText(tempFile, "content");

        var settings = new AddAdrSettings { Title = "A Decision", TemplatePath = tempFile, };

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
        var settings = new AddAdrSettings { Title = "A Decision", TemplatePath = "non-existent-file.md", };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldBe("The template path 'non-existent-file.md' does not exist.");
    }

    [Fact]
    public void Validate_Should_Return_Success_When_TemplatePath_Is_Not_Provided()
    {
        var settings = new AddAdrSettings { Title = "A Decision" };

        var result = settings.Validate();

        result.Successful.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Return_Error_When_Title_Is_Not_Provided()
    {
        // Arrange
        var settings = new AddAdrSettings { Title = null!, };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldBe("The title is required.");
    }

    [Fact]
    public void Validate_Should_Return_Error_When_Title_Is_Whitespace()
    {
        // Arrange
        var settings = new AddAdrSettings { Title = "   ", };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldBe("The title is required.");
    }
}
