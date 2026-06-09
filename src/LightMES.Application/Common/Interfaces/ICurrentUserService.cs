namespace LightMES.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);
}
