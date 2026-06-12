using LightMES.Domain.Common;
using LightMES.Domain.Enums;

namespace LightMES.Domain.Entities;

public class Material : AuditableEntity
{
    public string MaterialCode { get; private set; } = null!;
    public string MaterialName { get; private set; } = null!;

    public string? Specification { get; private set; }

    public string Unit { get; private set; } = null!;

    public MaterialType MaterialType { get; private set; }

    public bool IsActive { get; private set; }
    private Material() { }
    public Material(Guid id, string materialCode, string materialName, string? specification, string unit, MaterialType materialType, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(materialCode))
            throw new ArgumentException("物料编码不能为空", nameof(materialCode));
        if (string.IsNullOrWhiteSpace(materialName))
            throw new ArgumentException("物料名称不能为空", nameof(materialName));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("计量单位不能为空", nameof(unit));
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        MaterialCode = materialCode.ToUpper();
        MaterialName = materialName;
        Specification = specification;
        Unit = unit;
        MaterialType = materialType;
        IsActive = true;
        CreatedBy = createdBy;
        CreatedOn = DateTime.UtcNow;
    }
    public void Deactivate(string modifiedBy)
    {
        IsActive = false;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
    }
    public void Activate(string modifiedBy)
    {
        IsActive = true;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
    }
}
