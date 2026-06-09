using LightMES.Domain.Enums;

namespace LightMES.Application.Features.Equipments.Dtos;

public record EquipmentDto(
    Guid Id,
    string EquipmentCode,
    string EquipmentName,
    EquipmentStatus Status,
    string StatusText,
    string? Location,
    string? Description,
    bool IsActive,
    string CreateBy,
    DateTime CreateOn,
    string? LastModifiedBy,
    DateTime? LastModifiedOn);
