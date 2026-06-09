using FluentValidation;
using LightMES.Application.Common.Interfaces;
using MediatR;

namespace LightMES.Application.Features.Roles;

public record UpdateRoleCommand(Guid Id, string Name, string Description) : IRequest<bool>;
public class UpdateRole : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRole()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50).WithMessage("名称长度超出范围");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200).WithMessage("长度超出范围");
    }
}
public class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateRoleHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FindAsync(new object[] { request.Id }, cancellationToken);
        if (role == null) return false;
        role.Update(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}