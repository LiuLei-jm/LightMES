using LightMES.Domain.Common;
using LightMES.Domain.Enums;

namespace LightMES.Domain.Entities;

public class WorkOrder : AuditableEntity
{
    public string OrderNo { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public int PlanQty { get; private set; }
    public int CompletedQty { get; private set; }
    public int ScrapQty { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public Guid RouteId { get; private set; }
    public Route Route { get; private set; } = null!;

    public Guid? CurrentStepId { get; private set; }
    public RouteStep? CurrentStep { get; private set; } = null!;

    public Guid? CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    public Guid? CurrentOperatorId { get; private set; }
    public User? CurrentOperator { get; private set; }

    public DateTime PlannedStartTime { get; private set; }
    public DateTime PlannedEndTime { get; private set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }


    private readonly List<WorkOrderStepProgress> _stepProgresses = new();
    public IReadOnlyCollection<WorkOrderStepProgress> StepProgresses => _stepProgresses.AsReadOnly();
    private WorkOrder() { }
    public WorkOrder(Guid id, string orderNo, string productCode, string productName, int planQty, Guid routeId, Guid createdByUserId, DateTime planStart, DateTime planEnd)
    {
        Id = id;
        OrderNo = orderNo;
        ProductCode = productCode;
        ProductName = productName;
        PlanQty = planQty;
        RouteId = routeId;
        CreatedByUserId = createdByUserId;
        PlannedStartTime = planStart;
        PlannedEndTime = planEnd;
        Status = OrderStatus.Draft;
    }
    public void InitProgress(IEnumerable<RouteStep> steps)
    {
        _stepProgresses.Clear();
        foreach (var step in steps.OrderBy(s => s.Sequence))
        {
            _stepProgresses.Add(new WorkOrderStepProgress(Guid.NewGuid(), Id, step.Id, PlanQty));
        }
    }
    public void Release()
    {
        if (Status != OrderStatus.Draft) throw new InvalidOperationException("只有草稿状态的工单才能下发");
        Status = OrderStatus.Released;
    }
    public void Complete()
    {
        Status = OrderStatus.Completed;
        ActualEndTime = DateTime.UtcNow;
    }
    public void TrackIn(Guid stepId, Guid operatorId)
    {
        if (Status != OrderStatus.Released && Status != OrderStatus.Processing) throw new InvalidOperationException("当前工单状态无法进行开工过站");
        Status = OrderStatus.Processing;
        CurrentStepId = stepId;
        CurrentOperatorId = operatorId;
        if (ActualStartTime == null) ActualStartTime = DateTime.UtcNow;
    }
    public void ReportProgress(int okQty, int scrapQty)
    {
        CompletedQty += okQty;
        ScrapQty += scrapQty;
        if (CompletedQty >= PlanQty)
        {
            Status = OrderStatus.Completed;
            ActualEndTime = DateTime.UtcNow;
        }
    }
    public void UpdateDetails(int planQty, DateTime planStartTime, DateTime planEndTime)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("只有草稿状态的工单才允许进行修改生产计划！");
        if (planQty <= 0)
            throw new ArgumentException("计划产量必须大于 0");
        PlanQty = planQty;
        PlannedStartTime = planStartTime;
        PlannedEndTime = planEndTime;
    }
    public void Cancel()
    {
        if (Status == OrderStatus.Completed) throw new InvalidOperationException("已完工的工单无法撤销!");
        Status = OrderStatus.Canceled;
        ActualEndTime = DateTime.UtcNow;
    }
}
