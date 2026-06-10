using FluentValidation;
using LightMES.Application.Common.Interfaces;
using MediatR;

namespace LightMES.Application.Features.Users;

public record ChangeUserPasswordCommand(string OldPassword, string NewPassword) : IRequest<bool>;
public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("旧密码不能为空.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("密码至少6位.");
    }
}
public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public ChangeUserPasswordCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public IPasswordHasher PasswordHasher => _passwordHasher;

    public async Task<bool> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("Not authenticated.");
        Guid currentUserGuid = Guid.Parse(currentUserId);
        var user = await _context.Users.FindAsync(new object[] { currentUserGuid }, cancellationToken) ?? throw new KeyNotFoundException("User not found");
        var isOldPasswordValid = PasswordHasher.VerifyPassword(request.OldPassword, user.PasswordHash);
        if (!isOldPasswordValid) throw new Exception("Incorrect old password.");
        var passwordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.ChangePassword(passwordHash, user.Username);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
