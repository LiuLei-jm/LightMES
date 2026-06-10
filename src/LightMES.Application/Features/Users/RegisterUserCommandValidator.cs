using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Application.Common.Security;
using LightMES.Domain.Constants;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Users;

public record RegisterUserCommand(
    string Username,
    string Password,
    string FullName,
    string EmployeeNo,
    string? BadgeNo
    ) : IRequest<Guid>, ISecuredRequest
{
    public string RequiredPermission => Permissions.Users.Create;
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).WithMessage("用户名长度至少3位");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("密码长度至少6位");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("姓名不能为空");
        RuleFor(x => x.EmployeeNo).NotEmpty().WithMessage("员工工号不能为空");
    }
}
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    public RegisterUserCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username.ToLower(), cancellationToken))
        {
            throw new InvalidOperationException("用户名已存在");
        }
        if (await _context.Users.AnyAsync(u => u.EmployeeNo == request.EmployeeNo.ToUpper(), cancellationToken))
            throw new InvalidOperationException("该工号已经被注册");
        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var creator = _currentUserService.Username ?? "System";
        var user = new User(Guid.NewGuid(), request.Username, passwordHash, request.FullName, request.EmployeeNo, request.BadgeNo, creator);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}