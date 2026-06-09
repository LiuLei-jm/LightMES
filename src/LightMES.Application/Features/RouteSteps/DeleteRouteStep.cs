using LightMES.Application.Common.Interfaces;
using MediatR;

namespace LightMES.Application.Features.RouteSteps;

public record DeleteRouteStepCommand(Guid Id) : IRequest<Unit>;
public class DeleteRouteStepHandler : IRequestHandler<DeleteRouteStepCommand, Unit>
{
    private readonly IAppDbContext _context;

    public DeleteRouteStepHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteRouteStepCommand request, CancellationToken cancellationToken)
    {
        var step = await _context.RouteSteps.FindAsync(new object[] { request.Id }, cancellationToken);
        if (step == null) throw new KeyNotFoundException($"工序未找到：{request.Id}");
        _context.RouteSteps.Remove(step);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}