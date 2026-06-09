namespace LightMES.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string username, string fullName, IEnumerable<string> roles);
}
