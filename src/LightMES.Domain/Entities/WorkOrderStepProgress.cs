using LightMES.Domain.Common;
using LightMES.Domain.Enums;

namespace LightMES.Domain.Entities;

public class WorkOrderStepProgress : BaseEntity
{
    public Guid WorkOrderId { get; private set; }
    public Guid StepId { get; private set; }

    public StepStatus Status { get; private set; }
    public string? EquipmentCode { get; private set; }

    public int PlannedQty { get; private set; }
    public int InQueueQty { get; private set; }
    public int ProcessingQty { get; private set; }
    public int GoodQty { get; private set; }

    public int DefectiveQty { get; private set; }
    public int ScrapQty { get; private set; }

    private WorkOrderStepProgress() { }

    public WorkOrderStepProgress(Guid id, Guid workOrderId, Guid stepId, int plannedQty)
    {
        Id = id;
        WorkOrderId = workOrderId;
        StepId = stepId;
        PlannedQty = plannedQty;
        InQueueQty = plannedQty;
        ProcessingQty = 0;
        GoodQty = 0;
        DefectiveQty = 0;
        ScrapQty = 0;
    }

    public void RecordTrackIn()
    {
        if (InQueueQty > 0)
        {
            InQueueQty--;
            ProcessingQty++;
        }
        CheckCompletion();
    }

    public void RecordTrackOut()
    {
        if (ProcessingQty > 0)
        {
            ProcessingQty--;
            GoodQty++;
        }
        CheckCompletion();
    }

    public void RecordDefective()
    {
        if (ProcessingQty > 0)
        {
            ProcessingQty--;
            DefectiveQty++;
        }
        CheckCompletion();
    }

    public void RecordScrap()
    {
        if (ProcessingQty > 0)
        {
            ProcessingQty--;
            ScrapQty++;
        }
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (InQueueQty == 0 && ProcessingQty == 0)
            Status = StepStatus.Completed;
    }
}
