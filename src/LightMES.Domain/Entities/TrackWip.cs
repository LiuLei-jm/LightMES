using LightMES.Domain.Common;
using LightMES.Domain.Enums;

namespace LightMES.Domain.Entities;

public class TrackWip : BaseEntity
{
    public string SerialNumber { get; private set; } = null!;
    public Guid WorkOrderId { get; private set; }
    public Guid CurrentRouteId { get; private set; }
    public Guid CurrentStepId { get; private set; }
    public Guid CurrentWorkCenterId { get; private set; }

    public Guid OperatorId { get; private set; }
    public WipStatus Status { get; private set; }

    public DateTime TrackInTime { get; private set; }
    public DateTime? TrackOutTime { get; private set; }

    private TrackWip() { }

    public TrackWip(
        Guid id,
        string sn,
        Guid orderId,
        Guid routeId,
        Guid stepId,
        Guid workCenterId,
        Guid operatorId
    )
    {
        Id = id;
        SerialNumber = sn;
        WorkOrderId = orderId;
        CurrentRouteId = routeId;
        CurrentStepId = stepId;
        CurrentWorkCenterId = workCenterId;
        OperatorId = operatorId;
        Status = WipStatus.InQueue;
        TrackInTime = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        Status = WipStatus.Processing;
    }

    public void TrackIn(Guid operatorId)
    {
        if (Status != WipStatus.InQueue) throw new InvalidOperationException($"条码 {SerialNumber} 当前状态为 {Status}, 无法进站.");
        Status = WipStatus.Processing;
        OperatorId = operatorId;
        TrackInTime = DateTime.UtcNow;
    }
    public void TrackOut()
    {
        if (Status != WipStatus.Processing) throw new InvalidOperationException($"条码 {SerialNumber} 未进站，无法出站.");
        Status = WipStatus.Completed;
        TrackOutTime = DateTime.UtcNow;
    }

    public void MarkAsDefective()
    {
        Status = WipStatus.Defective;
    }
}
