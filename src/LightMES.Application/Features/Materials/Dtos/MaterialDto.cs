using LightMES.Domain.Enums;

namespace LightMES.Application.Features.Materials.Dtos;

public record MaterialDto(
    Guid Id,
    string MaterialCode,
    string MaterialName,
    string? Specification,
    string Unit,
    MaterialType MaterialType,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedOn,
    string? LastModifiedBy,
    DateTime? LastModifiedOn);
