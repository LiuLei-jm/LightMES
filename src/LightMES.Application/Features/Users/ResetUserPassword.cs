using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Application.Common.Security;
using LightMES.Domain.Constants;
using MediatR;

namespace LightMES.Application.Features.Users;

public record ResetUserPasswordCommand(Guid TargetUserId, string? NewPassword = "88888888") : IRequest<Unit>, ISecuredRequest
{
    public string NewPassword { get; init; } = string.IsNullOrWhiteSpace(NewPassword) ? "88888888" : NewPassword;
    public string RequiredPermission => Permissions.Users.ChangePassword;
}
public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword).MinimumLength(6).WithMessage("密码最小保持6位");
    }
}
public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public ResetUserPasswordCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.TargetUserId }, cancellationToken) ?? throw new KeyNotFoundException("User not found");
        var passwordHash = _passwordHasher.HashPassword(request.NewPassword);
        var currentUser = _currentUserService.Username ?? SystemConst.User.DefaultUser;
        user.ChangePassword(passwordHash, currentUser);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
