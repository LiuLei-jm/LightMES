using LightMES.Domain.Common;

namespace LightMES.Domain.Entities;

public class RouteStep : BaseEntity
{
    public Guid RouteId { get; private set; }
    public string StepCode { get; private set; } = null!;
    public string StepName { get; private set; } = null!;
    public int Sequence { get; private set; }
    public int StandardCycleTime { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public string? Description { get; private set; }
    private RouteStep() { }
    public RouteStep(Guid id, Guid routeId, string stepCode, string stepName, int sequence, int standardCycleTime, bool isRequired, string? description = "")
    {
        Id = id;
        RouteId = routeId;
        StepCode = stepCode;
        StepName = stepName;
        Sequence = sequence;
        StandardCycleTime = standardCycleTime;
        IsRequired = isRequired;
        Description = description;
    }
    public void Update(string stepCode, string stepName, int sequence, int standardCycleTime, bool isRequired, string? description)
    {
        StepCode = stepCode;
        StepName = stepName;
        Sequence = Sequence;
        StandardCycleTime = standardCycleTime;
        IsRequired = isRequired;
        Description = description;
    }
}
