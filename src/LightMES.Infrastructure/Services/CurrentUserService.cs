using LightMES.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LightMES.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppDbContext _context;

    private HashSet<string>? _cachedPermissions;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IAppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public async Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserId)) return false;
        if (_cachedPermissions == null)
        {
            var userIdGuid = Guid.Parse(UserId);
            var permissions = await _context.UserRoles.AsNoTracking()
                .Where(ur => ur.UserId == userIdGuid)
                .Select(ur => ur.Role)
                .SelectMany(r => r.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync(cancellationToken);
            _cachedPermissions = new HashSet<string>(permissions);
        }
        return _cachedPermissions.Contains(permission);
    }
}
