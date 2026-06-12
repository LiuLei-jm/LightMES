using FluentAssertions;
using LightMES.Domain.Entities;
using LightMES.Tests.Common;

namespace LightMES.Tests.Domain;

public class UserDomainTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly_AndNormalizeInputs()
    {
        //Arrange
        var id = Guid.NewGuid();
        var username = "Alex.Worker";
        var passwordHash = "Hash123!";
        var fullName = "Alex Mercer";
        var employeeNo = "emp-001";
        var badgeNo = "B1001";
        //Act
        var user = new User(id, username, passwordHash, fullName, employeeNo, badgeNo, TestConstants.Audit.CreatedBy);
        //Assert
        user.Id.Should().Be(id);
        user.Username.Should().Be("alex.worker");
        user.PasswordHash.Should().Be(passwordHash);
        user.FullName.Should().Be(fullName);
        user.EmployeeNo.Should().Be("EMP-001");
        user.BadgeNo.Should().Be(badgeNo);
        user.IsActive.Should().BeTrue();
        user.UserRoles.Should().BeEmpty();

        user.CreatedBy.Should().Be(TestConstants.Audit.CreatedBy);
        user.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        user.LastModifiedBy.Should().BeNull();
        user.LastModifiedOn.Should().BeNull();
    }
    [Fact]
    public void UpdateProfile_ShouldUpdateFields_AndNormalizeEmployeeNo()
    {
        //Arrange
        var user = CreateTestUser();
        //Act
        user.UpdateProfile("Alex Mercer New", "emp-999", "B9999", TestConstants.Audit.ModifiedBy);
        //Assert
        user.FullName.Should().Be("Alex Mercer New");
        user.EmployeeNo.Should().Be("EMP-999");
        user.BadgeNo.Should().Be("B9999");

        user.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        user.LastModifiedOn.Should().NotBeNull();
        user.LastModifiedOn.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void ChangePassword_ShouldUpdatePasswordHash_AndSetAuditFields()
    {
        //Arrange
        var user = CreateTestUser();
        var newPasswordHash = "NewSecureHash123!";
        //Act
        user.ChangePassword(newPasswordHash, TestConstants.Audit.ModifiedBy);
        //Assert
        user.PasswordHash.Should().Be(newPasswordHash);

        user.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        user.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_AndSetAuditFields()
    {
        //Arrange
        var user = CreateTestUser();
        user.IsActive.Should().BeTrue();
        //Act
        user.Deactivate(TestConstants.Audit.ModifiedBy);
        //Assert
        user.IsActive.Should().BeFalse();

        user.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        user.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void Activate_ShouldSetIsActiveToTrue_AndSetAuditFields()
    {
        //Arrage
        var user = CreateTestUser();
        user.Deactivate(TestConstants.Audit.ModifiedBy);
        user.IsActive.Should().BeFalse();
        //Act
        user.Activate(TestConstants.Audit.ModifiedBy);
        //Assert
        user.IsActive.Should().BeTrue();

        user.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        user.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void UpdateCardNo_ShouldUpdateBadgeNo_AndSetAuditFields()
    {
        //Arrange
        var user = CreateTestUser();
        var newCardNo = "NEW-CARD-777";
        //Act
        user.UpdateCardNo(newCardNo, TestConstants.Audit.ModifiedBy);
        //Assert
        user.BadgeNo.Should().Be(newCardNo);

        user.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        user.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void Delete_ShouldSetIsDeletedToTrue_AndSetAuditFields()
    {
        //Arrage
        var user = CreateTestUser();
        user.IsDeleted.Should().BeFalse();
        //Act
        user.Delete(TestConstants.Audit.ModifiedBy);
        //Assert
        user.IsDeleted.Should().BeTrue();

        user.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        user.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    #region Helper Methods (辅助方法)
    private static User CreateTestUser()
    {
        return new User(
            id: Guid.NewGuid(),
            username: TestConstants.Users.DefaultUsername,
            passwordHash: TestConstants.Users.DefaultPasswordHash,
            fullName: TestConstants.Users.DefaultFullName,
            employeeNo: TestConstants.Users.DefaultEmployeeNo,
            badgeNo: TestConstants.Users.DefaultBadgeNo,
            createdBy: TestConstants.Audit.CreatedBy
            );
    }
    #endregion
}
