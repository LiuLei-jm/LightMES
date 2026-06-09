using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.WorkOrders;

public record GetWorkOrderQuery(OrderStatus? Status, string? SearchTerm, int PageSize = 10, int PageNumber = 1) : IRequest<PagedList<WorkOrderDto>>;
public record WorkOrderDto(Guid id, string OrderNo, string ProductCode, string ProductName, int PlanQty, OrderStatus Status, DateTime PlanStartTime, double ProgressRate);
public record PagedList<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);
public class GetWorkOrderHandler : IRequestHandler<GetWorkOrderQuery, PagedList<WorkOrderDto>>
{
    private readonly IAppDbContext _context;

    public GetWorkOrderHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<WorkOrderDto>> Handle(GetWorkOrderQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkOrders.AsNoTracking();
        if (request.Status.HasValue) query = query.Where(w => w.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(w => w.OrderNo.Contains(request.SearchTerm) || w.ProductCode.Contains(request.SearchTerm));
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(w => w.PlannedStartTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WorkOrderDto(
                w.Id,
                w.OrderNo,
                w.ProductCode,
                w.ProductName,
                w.PlanQty,
                w.Status,
                w.PlannedStartTime,
                w.StepProgresses.Any() ? (double)w.StepProgresses.Count(sp => sp.Status == StepStatus.Completed) / w.StepProgresses.Count * 100 : 0))
            .ToListAsync(cancellationToken);
        return new PagedList<WorkOrderDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
