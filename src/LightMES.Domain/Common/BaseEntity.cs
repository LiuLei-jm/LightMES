namespace LightMES.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = default!;
    public uint Version { get; private set; }
}
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedOn { get; protected set; } = DateTime.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTime? LastModifiedOn { get; protected set; }
    public string? LastModifiedBy { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;
    public void MarkDeleted(string deletedBy)
    {
        IsDeleted = true;
        LastModifiedOn = DateTime.UtcNow;
        LastModifiedBy = deletedBy;
    }
}
