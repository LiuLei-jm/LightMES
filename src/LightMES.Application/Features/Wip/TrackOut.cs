using LightMES.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Wip;

public record TrackOutCommand(string SerialNumber, bool IsDefective) : IRequest<Unit>;
public record TrackOutResult(bool Success, Unit unit, string Message = "");
public class TrackOutCommandHandler : IRequestHandler<TrackOutCommand, Unit>
{
    private readonly IAppDbContext _context;

    public TrackOutCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Unit> Handle(TrackOutCommand request, CancellationToken cancellationToken)
    {
        var wip = await _context.TrackWips
            .FirstOrDefaultAsync(w => w.SerialNumber == request.SerialNumber && w.Status == Domain.Enums.WipStatus.Processing, cancellationToken);
        if (wip == null)
            throw new InvalidOperationException($"未找到条码 {request.SerialNumber} 的加工中记录，请先执行进站(Track In)");

        if (request.IsDefective) wip.MarkAsDefective();
        else wip.TrackOut();
        var progress = await _context.WorkOrderStepProgresses
            .FirstOrDefaultAsync(p => p.WorkOrderId == wip.WorkOrderId && p.StepId == wip.CurrentStepId, cancellationToken);
        if (progress != null)
        {
            if (request.IsDefective) progress.RecordDefective();
            else progress.RecordTrackOut();
        }
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
