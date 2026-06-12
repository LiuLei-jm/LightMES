using LightMES.Domain.Common;

namespace LightMES.Domain.Entities;

public class Route : AuditableEntity
{
    public string RouteCode { get; private set; } = null!;
    public string RouteName { get; private set; } = null!;
    public string Version { get; private set; } = null!;
    public bool IsActive { get; private set; }
    private readonly List<RouteStep> _steps = new();
    public IReadOnlyCollection<RouteStep> Steps => _steps.AsReadOnly();
    private Route() { }
    public Route(Guid id, string routeCode, string routeName, string version, string createdBy)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        RouteCode = routeCode;
        RouteName = routeName;
        Version = version;
        IsActive = true;
        CreatedBy = createdBy;
        CreatedOn = DateTime.UtcNow;
    }
    public void Update(string routeCode, string routeName, string version, bool isActive, string modifiedBy)
    {
        RouteCode = routeCode;
        RouteName = routeName;
        Version = version;
        IsActive = isActive;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
    }
    public void Delete()
    {
        IsDeleted = true;
    }
    public void AddStep(string stepCode, string stepName, int sequence, int standardCycleTime, bool isRequired)
    {
        if (_steps.Any(s => s.StepCode == stepCode.ToUpper())) throw new InvalidOperationException($"工序编码 {stepCode} 在该工艺路线中已存在");
        if (_steps.Any(s => s.Sequence == sequence)) throw new InvalidOperationException($"序号 {sequence} 已存在于该工艺路线中");
        var step = new RouteStep(Guid.NewGuid(), Id, stepCode, stepName, sequence, standardCycleTime, isRequired);
        _steps.Add(step);
    }
    public void RemoveStep() => _steps.Clear();
}
