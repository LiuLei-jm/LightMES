using LightMES.Application.Common.Interfaces;
using LightMES.Application.Common.Security;
using LightMES.Domain.Constants;
using MediatR;

namespace LightMES.Application.Features.Users;

public record DeleteUserCommand(
    Guid Id,
    string ModifiedBy) : IRequest<bool>, ISecuredRequest
{
    public string RequiredPermission => Permissions.Users.Delete;
}

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    public DeleteUserHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.Id }, cancellationToken);
        if (user == null) return false;
        if (user.Username == "admin")
        {
            throw new InvalidOperationException("系统安全保护：不能删除超级管理员账号。");
        }
        var operatorName = _currentUserService.Username ?? "System";
        user.Delete(operatorName);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}