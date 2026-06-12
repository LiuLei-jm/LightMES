using LightMES.Domain.Common;

namespace LightMES.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RolePermission> RolePermissions { get; private set; } =
        [];

    private Role() { }

    public Role(Guid id, string name, string description)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name;
        Description = description;
    }
    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
