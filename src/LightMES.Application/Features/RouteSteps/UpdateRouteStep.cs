using FluentValidation;
using LightMES.Application.Common.Interfaces;
using MediatR;

namespace LightMES.Application.Features.RouteSteps;

public record UpdateRouteStepCommand(
    Guid Id,
    string StepCode,
    string StepName,
    int Sequence,
    int StandardCycleTime,
    bool IsRequired,
    string? Description) : IRequest<Unit>;
public class UpdateRouteStepValidator : AbstractValidator<UpdateRouteStepCommand>
{
    public UpdateRouteStepValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.StepCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StepName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Sequence).GreaterThan(0);
        RuleFor(x => x.StandardCycleTime).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRouteStepHandler : IRequestHandler<UpdateRouteStepCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UpdateRouteStepHandler(IAppDbContext context) => _context = context;

    public async Task<Unit> Handle(UpdateRouteStepCommand request, CancellationToken cancellationToken)
    {
        var step = await _context.RouteSteps.FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new KeyNotFoundException($"工序未找到：{request.Id}");
        var exists = _context.RouteSteps.Any(s => s.RouteId == step.RouteId && s.Id != step.Id && (s.Sequence == request.Sequence || s.StepCode == request.StepCode));
        if (exists) throw new InvalidOperationException("修改后的工序号或工序编码冲突.");
        step.Update(
            request.StepCode,
            request.StepName,
            request.Sequence,
            request.StandardCycleTime,
            request.IsRequired,
            request.Description
            );
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}