namespace LightMES.Domain.Common;

public static class SystemRoles
{
    public static readonly Guid AdminId = Guid.Parse("A1111111-1111-1111-1111-111111111111");
    public const string AdminName = "Administrator";
    public static readonly Guid SupervisorId = Guid.Parse("B2222222-2222-2222-2222-222222222222");
    public const string SupervisorName = "Supervisor";
    public static readonly Guid OperatorId = Guid.Parse("C3333333-3333-3333-3333-333333333333");
    public const string OperatorName = "Operator";
    public static readonly Guid QCId = Guid.Parse("D4444444-4444-4444-4444-444444444444");
    public const string QCName = "QC";
    public static readonly Guid PlannerId = Guid.Parse("E5555555-5555-5555-5555-555555555555");
    public const string PlannerName = "Planner";
}
