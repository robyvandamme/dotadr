// Copyright © 2025 Roby Van Damme.

using System.Reflection;
using System.Reflection.Emit;
using DotAdr.Common;
using Shouldly;

namespace DotAdr.Tests.Common;

public class VersionInfoTests
{
    [Fact]
    public void Throws_When_Assembly_Is_Null()
    {
        Should.Throw<ArgumentNullException>(() => new VersionInfo(null!));
    }

    [Fact]
    public void Reads_Version_Metadata_From_Assembly()
    {
        var versionInfo = new VersionInfo(typeof(VersionInfo).Assembly);

        versionInfo.AssemblyVersion.ShouldNotBeNullOrWhiteSpace();
        versionInfo.AssemblyFileVersionInfo.ShouldNotBeNullOrWhiteSpace();
        versionInfo.ProductVersion.ShouldNotBeNullOrWhiteSpace();
        versionInfo.Version.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("1.2.3", "1.2.3")]
    [InlineData("1.2.3+commit", "1.2.3")]
    public void Parses_Product_Version(string? productVersion, string? expectedVersion)
    {
        VersionInfo.ParseProductVersion(productVersion).ShouldBe(expectedVersion);
    }

    [Theory]
    [InlineData("1.2.3+commit", "1.2.3.4", "1.2.3.5", "1.2.3")]
    [InlineData(null, "1.2.3.4", "1.2.3.5", "1.2.3.4")]
    [InlineData("", "1.2.3.4", "1.2.3.5", "1.2.3.4")]
    [InlineData(" ", "1.2.3.4", "1.2.3.5", "1.2.3.4")]
    [InlineData(null, null, "1.2.3.5", "1.2.3.5")]
    [InlineData(null, "", "1.2.3.5", "1.2.3.5")]
    [InlineData(null, null, null, "not available")]
    [InlineData("", "", "", "not available")]
    public void Resolves_Version_With_Fallback(
        string? productVersion,
        string? fileVersion,
        string? assemblyVersion,
        string expectedVersion)
    {
        VersionInfo.ResolveVersion(productVersion, fileVersion, assemblyVersion)
            .ShouldBe(expectedVersion);
    }

    [Fact]
    public void Removes_Build_Metadata_From_Assembly_Product_Version()
    {
        var versionInfo = new VersionInfo(Assembly.GetExecutingAssembly());

        versionInfo.ProductVersion.ShouldNotBeNullOrWhiteSpace();
        versionInfo.Version.ShouldBe(
            VersionInfo.ParseProductVersion(versionInfo.ProductVersion));
    }

    [Fact]
    public void Falls_Back_To_Assembly_Version_When_Assembly_Has_No_Location()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("VersionInfoTests.Dynamic")
            {
                Version = new Version(1, 2, 3, 4),
            },
            AssemblyBuilderAccess.Run);

        var versionInfo = new VersionInfo(assembly);

        versionInfo.AssemblyFileVersionInfo.ShouldBeNull();
        versionInfo.ProductVersion.ShouldBeNull();
        versionInfo.Version.ShouldBe("1.2.3.4");
    }
}
