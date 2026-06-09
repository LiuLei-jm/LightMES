using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Roles;

public record BindUserRoleCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;
public class BindUserRoleCommandHandler : IRequestHandler<BindUserRoleCommand, Unit>
{
    private readonly IAppDbContext _context;

    public BindUserRoleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(BindUserRoleCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        if (!userExists) throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        if (!roleExists) throw new KeyNotFoundException($"Role with ID {request.RoleId} not found");
        var alreadyBound = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);
        if (alreadyBound) return Unit.Value;
        var userRole = new UserRole { UserId = request.UserId, RoleId = request.RoleId };
        await _context.UserRoles.AddAsync(userRole, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}