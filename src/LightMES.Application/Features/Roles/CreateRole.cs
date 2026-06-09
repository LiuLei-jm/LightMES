using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Roles;

public record CreateRoleCommand(string Name, string Description) : IRequest<Guid>;
public class CreateRole : AbstractValidator<CreateRoleCommand>
{
    public CreateRole()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50).WithMessage("名称长度超出范围");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200).WithMessage("长度超出范围");
    }
}
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateRoleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken)) throw new InvalidOperationException($"Role with name '{request.Name}' already exists.");
        var role = new Role(
            Guid.NewGuid(),
            request.Name,
            request.Description);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync(cancellationToken);
        return role.Id;
    }
}