using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Common;
using MediatR;

namespace LightMES.Application.Features.Roles;

public record DeleteRoleCommand(Guid Id) : IRequest<bool>;
public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteRoleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FindAsync(new object[] { request.Id }, cancellationToken);
        if (role == null) return false;
        var isAdminRole = role.UserRoles.Any(ur => ur.RoleId == SystemRoles.AdminId);
        if (isAdminRole) throw new InvalidOperationException("系统完全保护：不能删除管理员角色");
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}