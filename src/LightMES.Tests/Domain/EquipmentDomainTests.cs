using FluentAssertions;
using LightMES.Domain.Common.Exceptions;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using LightMES.Tests.Common;

namespace LightMES.Tests.Domain;

public class EquipmentDomainTests
{
    [Fact]
    public void ChangeStatus_FromMaintenanceToRunning_ShouldThrowDomainException()
    {
        //Arrange
        var equipment = CreateTestEquipment();
        equipment.ChangeStatus(EquipmentStatus.Maintenance, TestConstants.Audit.ModifiedBy);
        //Act
        Action act = () =>
            equipment.ChangeStatus(EquipmentStatus.Running, TestConstants.Audit.ModifiedBy);
        //Assert
        act.Should()
            .Throw<InvalidStatusTransitionException>()
            .WithMessage(
                "Equipment must be verified (set to Idle) after maintenance before it can start running."
            );
    }

    [Fact]
    public void ChangeStatus_FromMaintenanceToIdle_ShouldSucceed()
    {
        //Arrange
        var equipment = CreateTestEquipment();
        equipment.ChangeStatus(EquipmentStatus.Maintenance, TestConstants.Audit.ModifiedBy);
        //Act
        equipment.ChangeStatus(EquipmentStatus.Idle, TestConstants.Audit.ModifiedBy);
        //Assert
        equipment.Status.Should().Be(EquipmentStatus.Idle);
        equipment.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        equipment.LastModifiedOn.Should().NotBeNull();
    }

    [Fact]
    public void ChangeStatus_WhenEquipmentIsInactive_ShouldThrowDomainException()
    {
        //Arrange
        var equipment = CreateTestEquipment();
        equipment.Deactivate("System");
        //Act
        Action act = () =>
            equipment.ChangeStatus(EquipmentStatus.Running, TestConstants.Audit.ModifiedBy);
        //Assert
        act.Should()
            .Throw<InvalidStatusTransitionException>()
            .WithMessage("Cannot change status of inactive equipment*");
    }

    [Fact]
    public void UpdateInfo_ShouldUpdateFields_AndSetAuditFields()
    {
        //Arrange
        var equipment = CreateTestEquipment();
        //Act
        equipment.UpdateInfo("测试CNC #2", "车间B 002", "修改描述", TestConstants.Audit.ModifiedBy);
        //Assert
        equipment.EquipmentName.Should().Be("测试CNC #2");
        equipment.Location.Should().Be("车间B 002");
        equipment.Description.Should().Be("修改描述");

        equipment.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        equipment.LastModifiedOn.Should().NotBeNull();
        equipment.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_AndSetAuditFields()
    {
        //Arrange
        var equipment = CreateTestEquipment();
        equipment.IsActive.Should().BeTrue();
        //Act
        equipment.Deactivate(TestConstants.Audit.ModifiedBy);
        //Assert
        equipment.IsActive.Should().BeFalse();

        equipment.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        equipment.LastModifiedOn.Should().NotBeNull();
        equipment.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue_AndSetAuditFields()
    {
        //Arrange
        var equipment = CreateTestEquipment();
        equipment.Deactivate(TestConstants.Audit.ModifiedBy);
        equipment.IsActive.Should().BeFalse();
        //Act
        equipment.Activate(TestConstants.Audit.ModifiedBy);
        //Assert
        equipment.IsActive.Should().BeTrue();

        equipment.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        equipment.LastModifiedOn.Should().NotBeNull();
        equipment.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    #region Helper Mothods (辅助方法)
    private static Equipment CreateTestEquipment()
    {
        return new Equipment(
            Guid.NewGuid(),
            TestConstants.Equipments.EquipmentCode,
            TestConstants.Equipments.EquipmentName,
            TestConstants.Equipments.Location,
            TestConstants.Equipments.Description,
            TestConstants.Audit.CreatedBy
        );
    }
    #endregion
}
