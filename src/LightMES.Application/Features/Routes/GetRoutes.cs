using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Routes.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Routes;

public record GetRoutesQuery() : IRequest<List<RouteListDto>>;

public record GetRouteByIdQuery(Guid Id) : IRequest<RouteDetailDto>;

public class RouteQueryHandler
    : IRequestHandler<GetRoutesQuery, List<RouteListDto>>,
        IRequestHandler<GetRouteByIdQuery, RouteDetailDto>
{
    private readonly IAppDbContext _context;

    public RouteQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RouteListDto>> Handle(
        GetRoutesQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Routes.AsNoTracking()
            .Select(r => new RouteListDto(
                r.Id,
                r.RouteCode,
                r.RouteName,
                r.Version,
                r.IsActive,
                _context.RouteSteps.Count(s => s.RouteId == r.Id)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<RouteDetailDto> Handle(
        GetRouteByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var route = await _context
            .Routes.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (route == null)
            throw new KeyNotFoundException($"未找到工艺路线: {request.Id}");
        var steps = await _context
            .RouteSteps.AsNoTracking()
            .Where(s => s.RouteId == request.Id)
            .OrderBy(s => s.Sequence)
            .Select(s => new RouteStepDetailDto(
                s.Id,
                s.StepCode,
                s.StepName,
                s.Sequence,
                s.StandardCycleTime,
                s.IsRequired,
                s.Description
            ))
            .ToListAsync(cancellationToken);
        return new RouteDetailDto(
            route.Id,
            route.RouteCode,
            route.RouteName,
            route.Version,
            route.IsActive,
            steps
        );
    }
}

