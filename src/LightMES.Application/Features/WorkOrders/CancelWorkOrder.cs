using LightMES.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.WorkOrders;

public record CancelWorkOrderCommand(Guid Id) : IRequest<CancelResult>;
public record CancelResult(bool Success, string ErrorMessage = "");
public class CancelWorkOrderHandler : IRequestHandler<CancelWorkOrderCommand, CancelResult>
{
    private readonly IAppDbContext _context;

    public CancelWorkOrderHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CancelResult> Handle(CancelWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var wo = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wo == null) return new CancelResult(false, "未找到该工单");
        try
        {
            wo.Cancel();
            await _context.SaveChangesAsync(cancellationToken);
            return new CancelResult(true);
        }
        catch (InvalidOperationException ex)
        {
            return new CancelResult(false, ex.Message);
        }
    }
}
