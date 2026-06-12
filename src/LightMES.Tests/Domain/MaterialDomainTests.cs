using FluentAssertions;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using LightMES.Tests.Common;

namespace LightMES.Tests.Domain;

public class MaterialDomainTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldInitializeCorrectly_AndNormalizeCode()
    {
        //Arrange
        var id = Guid.NewGuid();
        //Act
        var material = new Material(
            id,
            TestConstants.Materials.DefaultCode,
            TestConstants.Materials.DefaultName,
            TestConstants.Materials.DefaultSpec,
            TestConstants.Materials.DefaultUnit,
            MaterialType.RawMaterial,
            TestConstants.Audit.CreatedBy
            );
        //Assert
        material.Id.Should().Be(id);

        material.MaterialCode.Should().Be(TestConstants.Materials.ExpectedNormalizedCode);

        material.MaterialName.Should().Be(TestConstants.Materials.DefaultName);
        material.Specification.Should().Be(TestConstants.Materials.DefaultSpec);
        material.Unit.Should().Be(TestConstants.Materials.DefaultUnit);
        material.MaterialType.Should().Be(MaterialType.RawMaterial);

        material.IsActive.Should().BeTrue();
        material.CreatedBy.Should().Be(TestConstants.Audit.CreatedBy);
        material.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void Constructor_WithEmptyId_ShouldGenerateNewGuid()
    {
        //Act
        var material = new Material(
            Guid.Empty,
            TestConstants.Materials.DefaultCode,
            TestConstants.Materials.DefaultName,
            TestConstants.Materials.DefaultSpec,
            TestConstants.Materials.DefaultUnit,
            MaterialType.RawMaterial,
            TestConstants.Audit.CreatedBy
            );
        //Assert
        material.Id.Should().NotBe(Guid.Empty);
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_WhenMaterialCodeIsInvalid_ShouldThrowArgumentException(string? invalidCode)
    {
        //Act
        Action act = () => new Material(
            Guid.NewGuid(),
            invalidCode!,
            TestConstants.Materials.DefaultName,
            TestConstants.Materials.DefaultSpec,
            TestConstants.Materials.DefaultUnit,
            MaterialType.RawMaterial,
            TestConstants.Audit.CreatedBy
            );
        //Assert
        var exception = act.Should().Throw<ArgumentException>().And;
        exception.Message.Should().StartWith("物料编码不能为空");
        exception.ParamName.Should().Be("materialCode");
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_WhenMaterialNameIsInvalid_ShouldThrowArgumentException(string? invalidName)
    {
        //Act
        Action act = () => new Material(
            Guid.NewGuid(),
            TestConstants.Materials.DefaultCode,
            invalidName!,
            TestConstants.Materials.DefaultSpec,
            TestConstants.Materials.DefaultUnit,
            MaterialType.RawMaterial,
            TestConstants.Audit.CreatedBy
            );
        //Assert
        var exception = act.Should().Throw<ArgumentException>().And;
        exception.Message.Should().StartWith("物料名称不能为空");
        exception.ParamName.Should().Be("materialName");
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_WhenUnitIsInvalid_ShouldThrowArgumentException(string? invalidUnit)
    {
        //Act
        Action act = () => new Material(
            Guid.NewGuid(),
            TestConstants.Materials.DefaultCode,
            TestConstants.Materials.DefaultName,
            TestConstants.Materials.DefaultSpec,
            invalidUnit!,
            MaterialType.RawMaterial,
            TestConstants.Audit.CreatedBy
            );
        //Assert
        var exception = act.Should().Throw<ArgumentException>().And;
        exception.Message.Should().StartWith("计量单位不能为空");
        exception.ParamName.Should().Be("unit");
    }
    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_AndUpdateAuditFields()
    {
        //Arrange
        var material = CreateTestMaterial();
    }

    #region Helper Methods
    private static Material CreateTestMaterial()
    {
        return new Material(
            Guid.NewGuid(),
            TestConstants.Materials.DefaultCode,
            TestConstants.Materials.DefaultName,
            TestConstants.Materials.DefaultSpec,
            TestConstants.Materials.DefaultUnit,
            MaterialType.RawMaterial,
            TestConstants.Audit.CreatedBy
            );
    }
    #endregion
}
