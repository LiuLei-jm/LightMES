using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;

namespace LightMES.Application.Features.RouteSteps;

public record CreateRouteStepCommand(
    Guid RouteId,
    string StepCode,
    string StepName,
    int Sequence,
    int StandardCycleTime,
    bool IsRequired,
    string? Description
    ) : IRequest<Guid>;
public class CreateRouteStepValidator : AbstractValidator<CreateRouteStepCommand>
{
    public CreateRouteStepValidator()
    {
        RuleFor(x => x.RouteId).NotEmpty();
        RuleFor(x => x.StepCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StepName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Sequence).GreaterThan(0);
        RuleFor(x => x.StandardCycleTime).GreaterThanOrEqualTo(0);
    }
}
public class CreateRouteStepHandler : IRequestHandler<CreateRouteStepCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateRouteStepHandler(IAppDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateRouteStepCommand request, CancellationToken cancellationToken)
    {
        var exists = _context.RouteSteps.Any(s => s.RouteId == request.RouteId && (s.Sequence == request.Sequence || s.StepCode == request.StepCode));
        if (exists) throw new InvalidOperationException("工序号或工序编码在此工艺路线中已存在.");
        var routeStep = new RouteStep(
            Guid.NewGuid(),
            request.RouteId,
            request.StepCode,
            request.StepName,
            request.Sequence,
            request.StandardCycleTime,
            request.IsRequired,
            request.Description);
        await _context.RouteSteps.AddAsync(routeStep);
        await _context.SaveChangesAsync(cancellationToken);
        return routeStep.Id;
    }
}