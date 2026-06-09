using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.WorkOrders;

public record GetWorkOrderDetailQuery(Guid Id) : IRequest<WorkOrderDetailDto?>;
public record WorkOrderDetailDto(Guid Id, string OrderNo, string ProductCode, string ProductName, int PlanQty, OrderStatus Status, List<StepProgressDto> RouteSteps);
public record StepProgressDto(Guid StepId, string StepName, int Sequence, StepStatus Status,
    int GoodQty, int ScrapQty, string? EquipmentCode);
public class GetWorkOrderDetailHandler : IRequestHandler<GetWorkOrderDetailQuery, WorkOrderDetailDto?>
{
    private readonly IAppDbContext _context;

    public GetWorkOrderDetailHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkOrderDetailDto?> Handle(GetWorkOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var wo = await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Route).ThenInclude(r => r.Steps)
            .Include(w => w.StepProgresses)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wo == null) return null;
        var stepDtos = wo.StepProgresses.Select(sp =>
        {
            var step = wo.Route.Steps.First(s => s.Id == sp.StepId);
            return new StepProgressDto(
                step.Id, step.StepName, step.Sequence, sp.Status, sp.GoodQty, sp.ScrapQty, sp.EquipmentCode);
        }).OrderBy(s => s.Sequence).ToList();
        return new WorkOrderDetailDto(wo.Id, wo.OrderNo, wo.ProductCode, wo.ProductName, wo.PlanQty, wo.Status, stepDtos);
    }
}
