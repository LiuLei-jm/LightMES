using LightMES.Domain.Common;
using LightMES.Domain.Common.Exceptions;
using LightMES.Domain.Enums;

namespace LightMES.Domain.Entities;

public class Equipment : AuditableEntity
{
    public string EquipmentCode { get; private set; } = null!;
    public string EquipmentName { get; private set; } = null!;

    public EquipmentStatus Status { get; private set; }

    public string? Location { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    private Equipment() { }
    public Equipment(Guid id, string equipmentCode, string equipmentName, string? location, string? description, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(equipmentCode))
            throw new ArgumentException("设备编码不能为空", nameof(equipmentCode));
        if (string.IsNullOrWhiteSpace(equipmentName))
            throw new ArgumentException("设备名称不能为空", nameof(equipmentName));
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        EquipmentCode = equipmentCode;
        EquipmentName = equipmentName;
        Location = location;
        Description = description;
        Status = EquipmentStatus.Idle;
        IsActive = true;

        CreatedBy = createdBy;
        CreatedOn = DateTime.UtcNow;
    }
    public void UpdateInfo(string equipmentName, string? location, string? description, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(equipmentName))
            throw new ArgumentException("设备名称不能为空", nameof(equipmentName));
        EquipmentName = equipmentName;
        Location = location;
        Description = description;

        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
    }
    public void ChangeStatus(EquipmentStatus newStatus, string modifiedBy)
    {
        if (Status == newStatus) return;
        if (!IsActive) throw new InvalidStatusTransitionException($"Cannot change status of inactive equipment '{EquipmentCode}'.");
        if (Status == EquipmentStatus.Maintenance && newStatus == EquipmentStatus.Running)
            throw new InvalidStatusTransitionException("Equipment must be verified (set to Idle) after maintenance before it can start running.");
        if (Status == EquipmentStatus.Down && (newStatus == EquipmentStatus.Running || newStatus == EquipmentStatus.Idle))
            throw new InvalidStatusTransitionException("Down equipment must undergo maintenance before returning to service.");
        Status = newStatus;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
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
