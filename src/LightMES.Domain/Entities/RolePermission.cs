namespace LightMES.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public string Permission { get; set; } = string.Empty;
}
