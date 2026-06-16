using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Materials;

public record DeactivateMaterialCommand(Guid Id) : IRequest<Unit>;

public class DeactivateMaterialCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<DeactivateMaterialCommand, Unit>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Unit> Handle(
        DeactivateMaterialCommand request,
        CancellationToken cancellationToken
    )
    {
        var materials =
            await _context.Materials.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"未找到 ID 未 {request.Id} 的物料");
        var currentUser = _currentUserService.Username ?? SystemConst.User.DefaultUser;
        materials.Deactivate(currentUser);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
