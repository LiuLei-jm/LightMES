using LightMES.Application.Common.Interfaces;
using LightMES.Application.Common.Security;
using LightMES.Domain.Constants;
using MediatR;

namespace LightMES.Application.Features.Users;

public record ToggleUserStatusCommand(
    Guid Id,
    bool IsActive) : IRequest<bool>, ISecuredRequest
{
    public string RequiredPermission => Permissions.Users.ToggleStatus;
}

public class ToggleUserStatusHandler : IRequestHandler<ToggleUserStatusCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleUserStatusHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.Id }, cancellationToken);
        if (user == null) return false;
        if (user.Username == "admin")
        {
            throw new InvalidOperationException("系统安全保护：不能禁用超级管理员账号。");
        }
        var operatorName = _currentUserService.Username ?? "System";
        if (request.IsActive) user.Activate(operatorName); else user.Deactivate(operatorName);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
