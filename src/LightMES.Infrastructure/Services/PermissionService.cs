using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;

namespace LightMES.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    public Task<List<string>> GetPermissionsByRoleAsync(UserRole role)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetPermissionsByRolesAsync(IEnumerable<UserRole> roles)
    {
        throw new NotImplementedException();
    }
}
