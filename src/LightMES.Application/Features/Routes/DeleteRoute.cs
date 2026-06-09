using LightMES.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Routes;

public record DeleteRouteCommand(Guid Id) : IRequest<Unit>;
public class DeleteRouteHandler : IRequestHandler<DeleteRouteCommand, Unit>
{
    private readonly IAppDbContext _context;

    public DeleteRouteHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await _context.Routes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (route == null) throw new KeyNotFoundException($"工艺路线未找到: {request.Id}");
        var isUsedByWorkOrder = await _context.WorkOrders.AnyAsync(wo => wo.RouteId == request.Id, cancellationToken);
        if (isUsedByWorkOrder) throw new InvalidOperationException("无法删除该工艺路线，因为已有生产工单引用了它。建议将其设为禁用 (IsActive = false) .");
        route.Delete();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}