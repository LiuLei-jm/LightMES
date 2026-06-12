using FluentAssertions;
using LightMES.Domain.Entities;
using LightMES.Tests.Common;

namespace LightMES.Tests.Domain;

public class RouteDomainTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        //Act
        var id = Guid.NewGuid();
        var route = new Route(
            id: id,
            routeCode: TestConstants.Routes.DefaultCode,
            routeName: TestConstants.Routes.DefaultName,
            version: TestConstants.Routes.DefaultVersion,
            createdBy: TestConstants.Audit.CreatedBy
        );
        //Assert
        route.Id.Should().Be(id);
        route.RouteCode.Should().Be(TestConstants.Routes.DefaultCode);
        route.RouteName.Should().Be(TestConstants.Routes.DefaultName);
        route.Version.Should().Be(TestConstants.Routes.DefaultVersion);
        route.IsActive.Should().BeTrue();
        route.Steps.Should().BeEmpty();

        route.CreatedBy.Should().Be(TestConstants.Audit.CreatedBy);
        route.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_ShouldUpdateFields_AndSetAuditFields()
    {
        //Arrange
        var route = CreateTestRoute();
        //Act
        route.Update("R-NEW-002", "新工艺路线", "V2.0", false, TestConstants.Audit.ModifiedBy);
        //Assert
        route.RouteCode.Should().Be("R-NEW-002");
        route.RouteName.Should().Be("新工艺路线");
        route.Version.Should().Be("V2.0");
        route.IsActive.Should().BeFalse();

        route.LastModifiedBy.Should().Be(TestConstants.Audit.ModifiedBy);
        route.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void AddStep_WithValidData_ShouldAddSuccessfully()
    {
        //Arrange
        var route = CreateTestRoute();
        //Act
        route.AddStep(
            TestConstants.Routes.Steps.Step01Code,
            TestConstants.Routes.Steps.Step01Name,
            sequence: 10,
            standardCycleTime: 30,
            isRequired: true
        );
        //Assert
        route.Steps.Should().ContainSingle();
        var step = route.Steps.First();
        step.StepCode.Should().Be(TestConstants.Routes.Steps.Step01Code);
        step.StepName.Should().Be(TestConstants.Routes.Steps.Step01Name);
        step.Sequence.Should().Be(10);
        step.StandardCycleTime.Should().Be(30);
    }

    [Fact]
    public void AddStep_WhenDuplicateStepCode_ShouldThrowInvalidOperationException()
    {
        //Arrange
        var route = CreateTestRoute();
        route.AddStep("OP10", "工序一", 20, 30, true);
        //Act
        Action act = () => route.AddStep("op10", "工序一副本", 20, 30, true);
        //Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("工序编码 op10 在该工艺路线中已存在");
    }

    [Fact]
    public void AddStep_WhenDuplicateSequence_ShouldThrowInvalidOperationException()
    {
        //Arrange
        var route = CreateTestRoute();
        route.AddStep("OP10", "工序一", 10, 30, true);
        //Act
        Action act = () => route.AddStep("OP20", "工序二", 10, 45, true);
        //Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("序号 10 已存在于该工艺路线中");
    }

    [Fact]
    public void RemoveStep_ShouldClearAllSteps()
    {
        //Arrange
        var route = CreateTestRoute();
        route.AddStep("OP10", "工序一", 10, 30, true);
        route.AddStep("OP20", "工序二", 20, 30, true);
        route.Steps.Should().HaveCount(2);
        //Act
        route.RemoveStep();
        //Assert
        route.Steps.Should().BeEmpty();
    }

    [Fact]
    public void Delete_ShouldSetIsDeletedToTrue()
    {
        //Arrange
        var route = CreateTestRoute();
        //Act
        route.Delete();
        //Assert
        route.IsDeleted.Should().BeTrue();
    }

    #region Helper Methods
    private static Route CreateTestRoute()
    {
        return new Route(
            id: Guid.NewGuid(),
            routeCode: TestConstants.Routes.DefaultCode,
            routeName: TestConstants.Routes.DefaultName,
            version: TestConstants.Routes.DefaultVersion,
            createdBy: TestConstants.Audit.CreatedBy
        );
    }
    #endregion
}
