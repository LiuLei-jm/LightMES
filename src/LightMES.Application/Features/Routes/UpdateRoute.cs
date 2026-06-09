using FluentValidation;
using LightMES.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Routes;

public record UpdateRouteCommand(
    Guid Id,
    string RouteCode,
    string RouteName,
    string Version,
    bool IsActive) : IRequest<Unit>;
public class UpdateRouteValidator : AbstractValidator<UpdateRouteCommand>
{
    public UpdateRouteValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RouteCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RouteName).NotEmpty().MaximumLength(100);
    }
}
public class UpdateRouteHnadler : IRequestHandler<UpdateRouteCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UpdateRouteHnadler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await _context.Routes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (route == null) throw new KeyNotFoundException($"工艺路线未找到：{request.Id}");
        var codeExists = await _context.Routes.AnyAsync(r => r.RouteCode == request.RouteCode && r.Id != request.Id, cancellationToken);
        if (codeExists) throw new InvalidOperationException($"工艺路线编码 '{request.RouteCode}' 已存在。");
        route.Update(request.RouteCode, request.RouteName, request.Version, request.IsActive, string.Empty);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}