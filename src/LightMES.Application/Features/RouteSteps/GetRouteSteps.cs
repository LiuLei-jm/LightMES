using LightMES.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.RouteSteps;

public record RouteStepDto(
    Guid Id,
    Guid RouteId,
    string StepCode,
    string StepName,
    int Sequence,
    int StandardCycleTime,
    bool IsRequeired,
    string? Description);

public record GetRouteStepByIdQuery(Guid Id) : IRequest<RouteStepDto>;
public record GetRouteStepsByRouteIdQuery(Guid RouteId) : IRequest<List<RouteStepDto>>;

public class RouteStepQueryHandler : IRequestHandler<GetRouteStepByIdQuery, RouteStepDto>, IRequestHandler<GetRouteStepsByRouteIdQuery, List<RouteStepDto>>
{
    private readonly IAppDbContext _context;

    public RouteStepQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RouteStepDto>> Handle(GetRouteStepsByRouteIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.RouteSteps.AsNoTracking()
            .Where(x => x.RouteId == request.RouteId)
            .OrderBy(x => x.Sequence)
            .Select(step => new RouteStepDto(
                step.Id,
                step.RouteId,
                step.StepCode,
                step.StepName,
                step.Sequence,
                step.StandardCycleTime,
                step.IsRequired,
                step.Description
                )).ToListAsync(cancellationToken);
    }

    public async Task<RouteStepDto> Handle(GetRouteStepByIdQuery request, CancellationToken cancellationToken)
    {
        var step = await _context.RouteSteps.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (step == null) throw new KeyNotFoundException("工序不存在");
        return new RouteStepDto(step.Id, step.RouteId, step.StepCode, step.StepName, step.Sequence, step.StandardCycleTime, step.IsRequired, step.Description);
    }
}