using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Wip;

public record TrackInCommand(string SerialNumber,
    Guid WorkOrderId,
    Guid RouteId,
    Guid StepId,
    Guid WorkCenterId,
    Guid OperatorId) : IRequest<Guid>;
public record TrackInResult(bool Success, Guid trackWipId, string Message = "");
public class TrackInCommandHandler : IRequestHandler<TrackInCommand, Guid>
{
    private readonly IAppDbContext _context;
    public TrackInCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Guid> Handle(TrackInCommand request, CancellationToken cancellationToken)
    {
        var activeWip = await _context.TrackWips
            .FirstOrDefaultAsync(w => w.SerialNumber == request.SerialNumber && (w.Status == WipStatus.InQueue || w.Status == WipStatus.Processing), cancellationToken);
        if (activeWip != null) throw new InvalidOperationException($"条码 {request.SerialNumber} 已经在工序加工中，无法重复进站");
        var wip = new TrackWip(
            Guid.NewGuid(),
            request.SerialNumber,
            request.WorkOrderId,
            request.RouteId,
            request.StepId,
            request.WorkCenterId,
            request.OperatorId
            );

        wip.StartProcessing();
        var progress = await _context.WorkOrderStepProgresses
            .FirstOrDefaultAsync(p => p.WorkOrderId == request.WorkOrderId && p.StepId == request.StepId, cancellationToken);
        if (progress == null)
        {
            progress = new WorkOrderStepProgress(Guid.NewGuid(), request.WorkOrderId, request.StepId, 100);
            await _context.WorkOrderStepProgresses.AddAsync(progress);
        }
        progress.RecordTrackIn();
        await _context.TrackWips.AddAsync(wip);
        await _context.SaveChangesAsync();
        return wip.Id;
    }
}
