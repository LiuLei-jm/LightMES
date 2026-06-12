using FluentAssertions;
using LightMES.Domain.Entities;
using LightMES.Tests.Common;

namespace LightMES.Tests.Domain;

public class WorkOrderDoaminTests
{
    private readonly Guid _routeId = Guid.NewGuid();
    private readonly Guid _creatorId = Guid.NewGuid();
    [Fact]
    public void Constructor_ShouldInitializeToDraftState()
    {
        //Arrange
        var id = Guid.NewGuid();
        var planStart = DateTime.UtcNow.AddDays(1);
        var planEnd = DateTime.UtcNow.AddDays(2);
        //Act
        var wo = new WorkOrder(
            id,
            TestConstants.WorkOrders.DefaultOrderNo,
            TestConstants.WorkOrders.ProductCode,
            TestConstants.WorkOrders.ProductName,
            TestConstants.WorkOrders.DefaultPlanQty,
            _routeId,
            _creatorId,
            planStart,
            planEnd
            );
        //Assert
        wo.Id.Should().Be(id);
        wo.OrderNo.Should().Be(TestConstants.WorkOrders.DefaultOrderNo);
        wo.Status.Should().Be(LightMES.Domain.Enums.OrderStatus.Draft);
        wo.PlanQty.Should().Be(TestConstants.WorkOrders.DefaultPlanQty);
        wo.CompletedQty.Should().Be(0);
        wo.ScrapQty.Should().Be(0);
        wo.ActualStartTime.Should().BeNull();
        wo.ActualEndTime.Should().BeNull();
        wo.StepProgresses.Should().BeEmpty();
    }
    [Fact]
    public void InitProgress_ShouldGenerateProgressInSequenceOrder()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        var steps = new List<RouteStep>
        {
            new RouteStep(Guid.NewGuid(), _routeId, "OP20", "外观检测",20, 30,true),
            new RouteStep(Guid.NewGuid(), _routeId, "OP10", "锡膏印刷",10, 20, true)
        };
        //Act
        wo.InitProgress(steps);
        //Assert
        wo.StepProgresses.Should().HaveCount(2);

        var progressList = wo.StepProgresses.ToList();
        progressList[0].StepId.Should().Be(steps[1].Id);
        progressList[1].StepId.Should().Be(steps[0].Id);
    }
    [Fact]
    public void Release_WhenDraft_ShouldTransitionToReleased()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        //Act
        wo.Release();
        //Assert
        wo.Status.Should().Be(LightMES.Domain.Enums.OrderStatus.Released);
    }
    [Fact]
    public void Release_WhenNotDraft_ShouldThrowInvalidOperationException()
    {
        //Arrage
        var wo = CreateDraftWorkOrder();
        wo.Release();
        //Act
        Action act = () => wo.Release();
        //Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("只有草稿状态的工单才能下发");
    }
    [Fact]
    public void TrackIn_WhenReleased_ShouldSetProcessingStatusAndStartTime()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        wo.Release();
        var step1Id = Guid.NewGuid();
        var operatorId = Guid.NewGuid();

        wo.TrackIn(step1Id, operatorId);
        var originalStartTime = wo.ActualStartTime;
        var step2Id = Guid.NewGuid();
        //Act
        Thread.Sleep(100);
        wo.TrackIn(step2Id, operatorId);
        //Assert
        wo.CurrentStepId.Should().Be(step2Id);
        wo.ActualStartTime.Should().Be(originalStartTime);
    }
    [Fact]
    public void TrackIn_WhenInvalidStatus_ShouldThrowInvalidOperationExcetption()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        //Act
        Action act = () => wo.TrackIn(Guid.NewGuid(), Guid.NewGuid());
        //Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("当前工单状态无法进行开工过站");
    }
    [Fact]
    public void ReportProgress_ShouldAccumulateQuatities()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        //Act
        wo.ReportProgress(okQty: 10, scrapQty: 2);
        wo.ReportProgress(okQty: 20, scrapQty: 1);
        //Assert
        wo.CompletedQty.Should().Be(30);
        wo.ScrapQty.Should().Be(3);
    }
    [Fact]
    public void ReportProgress_WhenCompletedQtyReachePlanQty_ShouldAutoComplete()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        //Act
        wo.ReportProgress(okQty: 100, scrapQty: 5);
        //Assert
        wo.Status.Should().Be(LightMES.Domain.Enums.OrderStatus.Completed);
        wo.ActualEndTime.Should().NotBeNull();
        wo.ActualEndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void UpdateDetails_WhenDraft_ShouldModifyDetails()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        var newPlanStart = DateTime.UtcNow.AddDays(3);
        var newPlanEnd = DateTime.UtcNow.AddDays(4);
        //Act
        wo.UpdateDetails(150, newPlanStart, newPlanEnd);
        //Assert
        wo.PlanQty.Should().Be(150);
        wo.PlannedStartTime.Should().Be(newPlanStart);
        wo.PlannedEndTime.Should().Be(newPlanEnd);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void UpdateDetails_WithInvalidQty_ShouldThrowArgumentException(int invalidQty)
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        //Act
        Action act = () => wo.UpdateDetails(invalidQty, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        //Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("计划产量必须大于 0");
    }
    [Fact]
    public void UpdateDetails_WhenNotDraft_ShouldThrowInvalidOperationException()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        wo.Release();
        //Act
        Action act = () => wo.UpdateDetails(200, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        //Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("只有草稿状态的工单才允许进行修改生产计划！");
    }
    [Fact]
    public void Cancel_WhenValidStatus_ShouldTransitionToCanceled()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        //Act
        wo.Cancel();
        //Assert
        wo.Status.Should().Be(LightMES.Domain.Enums.OrderStatus.Canceled);
        wo.ActualEndTime.Should().NotBeNull();
    }
    [Fact]
    public void Cnacel_WhenAlreadyComleted_ShouldThrowInvalidOperationException()
    {
        //Arrange
        var wo = CreateDraftWorkOrder();
        wo.Complete();
        //Act
        Action act = () => wo.Cancel();
        //Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("已完工的工单无法撤销!");
    }

    #region Helper Methods
    private WorkOrder CreateDraftWorkOrder()
    {
        return new WorkOrder(
            Guid.NewGuid(),
            TestConstants.WorkOrders.DefaultOrderNo,
            TestConstants.WorkOrders.ProductCode,
            TestConstants.WorkOrders.ProductName,
            TestConstants.WorkOrders.DefaultPlanQty,
            _routeId,
            _creatorId,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(10)
            );
    }
    #endregion
}
