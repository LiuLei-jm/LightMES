using FluentAssertions;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using LightMES.Tests.Common;

namespace LightMES.Tests.Domain;

public class TrackWipDomainTests
{
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _routeId = Guid.NewGuid();
    private readonly Guid _stepId = Guid.NewGuid();
    private readonly Guid _workCenterId = Guid.NewGuid();
    private readonly Guid _operatorId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        //Arrange
        var id = Guid.NewGuid();
        //Act
        var wip = new TrackWip(
            id,
            TestConstants.TrackWips.DefaultSN,
            _orderId,
            _routeId,
            _stepId,
            _workCenterId,
            _operatorId
        );
        //Assert
        wip.Id.Should().Be(id);
        wip.SerialNumber.Should().Be(TestConstants.TrackWips.DefaultSN);
        wip.WorkOrderId.Should().Be(_orderId);
        wip.CurrentRouteId.Should().Be(_routeId);
        wip.CurrentStepId.Should().Be(_stepId);
        wip.CurrentWorkCenterId.Should().Be(_workCenterId);
        wip.OperatorId.Should().Be(_operatorId);

        wip.Status.Should().Be(WipStatus.InQueue);
        wip.TrackInTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        wip.TrackOutTime.Should().BeNull();
    }

    [Fact]
    public void StartProcessing_ShouldSetStatusToProcessing()
    {
        // Arrange
        var wip = CreateInQueueWip();
        // Act
        wip.StartProcessing();
        // Assert
        wip.Status.Should().Be(WipStatus.Processing);
    }

    [Fact]
    public void TrackIn_WhenInQueue_SHouldTransitionToProcessing_AndUpdateOperatorAndTrackInTime()
    {
        // Arrange
        var wip = CreateInQueueWip();
        var newOperatorId = Guid.NewGuid();
        // Act
        wip.TrackIn(newOperatorId);
        // Assert
        wip.Status.Should().Be(WipStatus.Processing);
        wip.OperatorId.Should().Be(newOperatorId);
        wip.TrackInTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TrackIn_WhenAlreadyProcessing_ShouldThrwoInvalidOperationException()
    {
        // Arrange
        var wip = CreateInQueueWip();
        wip.StartProcessing();
        // Act
        Action act = () => wip.TrackIn(Guid.NewGuid());
        // Assert
        var exception = act.Should().Throw<InvalidOperationException>().And;
        exception.Message.Should().Contain(wip.SerialNumber);
        exception.Message.Should().Contain(wip.Status.ToString());
        exception.Message.Should().Contain("无法进站");
    }

    [Fact]
    public void TrackOut_WhenProcessing_ShouldTransitionToCompleted_AndSetTrackOutTime()
    {
        // Arrange
        var wip = CreateInQueueWip();
        wip.StartProcessing();
        // Act
        wip.TrackOut();
        // Assert
        wip.Status.Should().Be(WipStatus.Completed);
        wip.TrackOutTime.Should().NotBeNull();
        wip.TrackOutTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TrackOut_WhenNotInProcessing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var wip = CreateInQueueWip();
        // Act
        Action act = () => wip.TrackOut();
        // Assert
        var exception = act.Should().Throw<InvalidOperationException>().And;
        exception.Message.Should().Contain(wip.SerialNumber);
        exception.Message.Should().Contain("未进站，无法出站");
    }

    [Fact]
    public void MarkAsDefective_ShouldSetStatusToDefective()
    {
        // Arrange
        var wip = CreateInQueueWip();
        // Act
        wip.MarkAsDefective();
        // Assert
        wip.Status.Should().Be(WipStatus.Defective);
    }

    private TrackWip CreateInQueueWip()
    {
        return new TrackWip(
            Guid.NewGuid(),
            TestConstants.TrackWips.DefaultSN,
            _orderId,
            _routeId,
            _stepId,
            _workCenterId,
            _operatorId
        );
    }
}
