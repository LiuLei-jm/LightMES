using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Users;

public record LoginRequest(string? Username, string? Password, string? BadgeNo) : IRequest<LoginResult>;
public record LoginResult(bool Success, string Token = "", string Message = "", string FullName = "", ICollection<UserRole> Roles = null!);
public class LoginUserHandler : IRequestHandler<LoginRequest, LoginResult>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginUserHandler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResult> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        User? user = null;
        if (!string.IsNullOrWhiteSpace(request.BadgeNo))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.BadgeNo == request.BadgeNo && u.IsActive, cancellationToken);
            if (user == null) return new LoginResult(false, Message: "无效或禁用的工牌");
        }
        else if (!string.IsNullOrWhiteSpace(request.Username) && !string.IsNullOrWhiteSpace(request.Password))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken);
            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new LoginResult(false, Message: "用户名或密码错误");
            }
        }
        else
        {
            return new LoginResult(false, Message: "请输入登录凭证");
        }
        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _tokenGenerator.GenerateToken(user.Id, user.Username, user.FullName, roleNames);
        return new LoginResult(true, token, "登录成功", user.FullName, user.UserRoles);
    }
}
