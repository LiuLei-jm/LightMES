using LightMES.Domain.Entities;

namespace LightMES.Application.Features.Users.Dtos;

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    string EmployeeNo,
    string? BadgeNo,
    ICollection<UserRole> Roles,
    bool IsActive);
