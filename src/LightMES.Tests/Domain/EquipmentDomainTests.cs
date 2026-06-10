using FluentAssertions;
using LightMES.Domain.Common.Exceptions;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMES.Tests.Domain;

public class EquipmentDomainTests
{
    [Fact]
    public void ChangeStatus_FromMaintenanceToRunning_ShouldThrowDomainException()
    {
        //Arrange
        var equipment = new Equipment(Guid.NewGuid(), "CNC-01", "测试CNC #1", null, null,"System");
        equipment.ChangeStatus(LightMES.Domain.Enums.EquipmentStatus.Maintenance, "System");
        //Act
        Action act = () => equipment.ChangeStatus(LightMES.Domain.Enums.EquipmentStatus.Running, "System");
        //Assert
        act.Should().Throw<InvalidStatusTransitionException>().WithMessage("Equipment must be verified (set to Idle) after maintenance before it can start running.");
    }
    [Fact]
    public void ChangeStatus_FromMaintenanceToIdle_ShouldSucceed()
    {
        //Arrange
        var equipment = new Equipment(Guid.NewGuid(), "CNC-01", "测试CNC #1", null, null,"System");
        equipment.ChangeStatus(LightMES.Domain.Enums.EquipmentStatus.Maintenance, "System");
        //Act
        equipment.ChangeStatus(
            LightMES.Domain.Enums.EquipmentStatus.Idle,
            "Technician-A");
        //Assert
        equipment.Status.Should().Be(EquipmentStatus.Idle);
        equipment.LastModifiedBy.Should().Be("Technician-A");
        equipment.LastModifiedOn.Should().NotBeNull();
    }
    [Fact]
    public void ChangeStatus_WhenEquipmentIsInactive_ShouldThrowDomainException()
    {
        //Arrange
        var equipment = new Equipment(Guid.NewGuid(), "CNC-01", "测试CNC #1", null, null,"System");
        equipment.Deactivate("System");
        //Act
        Action act = () => equipment.ChangeStatus(EquipmentStatus.Running, "Operator-A");
        //Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Cannot change status of inactive equipment*");
    }
}
