using LightMES.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Roles;

public record UnbindUserRoleCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;
public class UnbindUserRoleHandler : IRequestHandler<UnbindUserRoleCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UnbindUserRoleHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UnbindUserRoleCommand request, CancellationToken cancellationToken)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);
        if (userRole == null) return Unit.Value;
        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}