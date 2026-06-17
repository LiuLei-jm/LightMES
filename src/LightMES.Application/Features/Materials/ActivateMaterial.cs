using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using MediatR;

namespace LightMES.Application.Features.Materials;

public record ActivateMaterialCommand(Guid Id) : IRequest<Unit>;

public class ActivateMaterialCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<ActivateMaterialCommand, Unit>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Unit> Handle(
        ActivateMaterialCommand request,
        CancellationToken cancellationToken
    )
    {
        var material =
            await _context.Materials.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"未找到 ID 为 {request.Id} 的物料");
        var currentUser = _currentUserService.Username ?? SystemConst.User.DefaultUser;
        material.Activate(currentUser);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
