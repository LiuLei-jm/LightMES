using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Roles;

public record UnbindRolePermissionCommand(Guid RoleId, string Permission) : IRequest<Unit>;
public class UnbindRolePermissionCommandValidator : AbstractValidator<UnbindRolePermissionCommand>
{
    public UnbindRolePermissionCommandValidator()
    {
        RuleFor(x => x.Permission)
            .NotEmpty().WithMessage("Permission connot be empty")
            .Must(Permissions.IsValidPermission)
            .WithMessage(x => $"Permission '{x.Permission}' is invalid. Please use a defined system permission.");
    }
}
public class UnbindRolePermissionCommandHandler : IRequestHandler<UnbindRolePermissionCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UnbindRolePermissionCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UnbindRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.Permission == request.Permission, cancellationToken);
        if (rolePermission == null) return Unit.Value;
        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}