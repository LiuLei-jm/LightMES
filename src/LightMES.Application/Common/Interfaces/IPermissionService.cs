using LightMES.Domain.Entities;

namespace LightMES.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<List<string>> GetPermissionsByRoleAsync(UserRole role);
    Task<List<string>> GetPermissionsByRolesAsync(IEnumerable<UserRole> roles);
}
