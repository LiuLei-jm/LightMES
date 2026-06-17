using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;

namespace LightMES.Application.Features.Routes;

public record RouteStepDto(
    string StepCode,
    string StepName,
    int Sequence,
    int StandardCycleTime,
    bool IsRequired
);

public record CreateRouteCommand(
    string RouteCode,
    string RouteName,
    string Version,
    List<RouteStepDto> Steps,
    string Creator
) : IRequest<Guid>;

public class CreateRouteCommandHandler : IRequestHandler<CreateRouteCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateRouteCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = new Route(
            Guid.NewGuid(),
            request.RouteCode,
            request.RouteName,
            request.Version,
            request.Creator
        );
        foreach (var step in request.Steps.OrderBy(s => s.Sequence))
        {
            route.AddStep(
                step.StepCode,
                step.StepName,
                step.Sequence,
                step.StandardCycleTime,
                step.IsRequired
            );
        }
        await _context.Routes.AddAsync(route);
        await _context.SaveChangesAsync(cancellationToken);
        return route.Id;
    }
}

