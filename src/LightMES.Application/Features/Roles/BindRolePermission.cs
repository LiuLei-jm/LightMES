using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Roles;

public record BindRolePermissionCommand(Guid RoleId, string Permission) : IRequest<Unit>;
public class BindRolePermissionCommandValidator : AbstractValidator<BindRolePermissionCommand>
{
    public BindRolePermissionCommandValidator()
    {
        RuleFor(x => x.Permission)
            .NotEmpty().WithMessage("Permission connot be empty.")
            .Must(Permissions.IsValidPermission)
            .WithMessage(x => $"Permission '{x.Permission}' is invalid. Please use a defined system permission.");
    }
}
public class BindRolePermissionCommandHandler : IRequestHandler<BindRolePermissionCommand, Unit>
{
    private readonly IAppDbContext _context;

    public BindRolePermissionCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(BindRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        if (!roleExists) throw new KeyNotFoundException($"Role with ID {request.RoleId} not found.");
        var alreadyBound = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == request.RoleId && rp.Permission == request.Permission, cancellationToken);
        if (alreadyBound) return Unit.Value;
        var rolePermission = new RolePermission { RoleId = request.RoleId, Permission = request.Permission };
        await _context.RolePermissions.AddAsync(rolePermission, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}