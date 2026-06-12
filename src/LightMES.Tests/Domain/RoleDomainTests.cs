using FluentAssertions;
using LightMES.Domain.Entities;
using LightMES.Tests.Common;

namespace LightMES.Tests.Domain;

public class RoleDomainTests
{
    [Fact]
    public void Constructor_WithValidId_ShouldInitializeCorrently()
    {
        //Arrange
        var id = Guid.NewGuid();
        var name = TestConstants.Roles.AdminName;
        var description = TestConstants.Roles.AdminDescription;
        //Act
        var role = new Role(id, name, description);
        //Assert
        role.Id.Should().Be(id);
        role.Name.Should().Be(name);
        role.Description.Should().Be(description);

        role.UserRoles.Should().NotBeNull().And.BeEmpty();
        role.RolePermissions.Should().NotBeNull().And.BeEmpty();
    }
    [Fact]
    public void Constructor_WithEmptyId_ShouldGenerateNewGuid()
    {
        //Arrange
        var emptyId = Guid.Empty;
        var name = TestConstants.Roles.OperatorName;
        var description = TestConstants.Roles.OperatorDescription;
        //Act
        var role = new Role(emptyId, name, description);
        //Assert
        role.Id.Should().NotBe(Guid.Empty);
        role.Name.Should().Be(name);
        role.Description.Should().Be(description);
    }
    [Fact]
    public void Update_ShouldModifyPropertiesSuccessfully()
    {
        //Arrange
        var role = CreateTestRole();
        var newName = "Senior Operator";
        var newDescription = "高级操作工，拥有额外的设备调试权限";
        //Act
        role.Update(newName, newDescription);
        //Assert
        role.Name.Should().Be(newName);
        role.Description.Should().Be(newDescription);

    }

    #region Helper Methods
    private static Role CreateTestRole()
    {
        return new Role(
            Guid.NewGuid(),
            TestConstants.Roles.OperatorName,
            TestConstants.Roles.OperatorDescription
            );
    }
    #endregion
}
