using LightMES.Application.Common.Interfaces;
using LightMES.Application.Common.Security;
using LightMES.Domain.Constants;
using MediatR;

namespace LightMES.Application.Features.Users;

public record UpdateUserCommand(
    Guid Id,
    string FullName,
    string EmployeeNo,
    string? BadgeNo,
    string ModifiedBy) : IRequest<bool>, ISecuredRequest
{
    public string RequiredPermission => Permissions.Users.Edit;
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.Id }, cancellationToken);
        if (user == null) return false;
        var modifier = _currentUserService.Username ?? "System";
        user.UpdateProfile(request.FullName, request.EmployeeNo, request.BadgeNo, modifier);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
