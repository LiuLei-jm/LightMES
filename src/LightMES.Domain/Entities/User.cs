using LightMES.Domain.Common;

namespace LightMES.Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string EmployeeNo { get; private set; } = null!;
    public string? BadgeNo { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    private User() { }
    public User(Guid id, string username, string passwordHash, string fullName, string employeeNo, string? badgeNo, string createdBy)
    {
        Id = id;
        Username = username.ToLower();
        PasswordHash = passwordHash;
        FullName = fullName;
        EmployeeNo = employeeNo.ToUpper();
        BadgeNo = badgeNo;
        CreatedBy = createdBy;
        CreatedOn = DateTime.UtcNow;
    }
    public void UpdateProfile(string fullName, string employeeNo, string? badgeNo, string modifiedBy)
    {
        FullName = fullName;
        EmployeeNo = employeeNo.ToUpper();
        BadgeNo = badgeNo;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
    }
    public void ChangePassword(string newPasswordHash, string modifiedBy)
    {
        PasswordHash = newPasswordHash;
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
    public void UpdateCardNo(string newCardNo, string modifiedBy)
    {
        BadgeNo = newCardNo;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
    }
    public void Delete(string modifiedBy)
    {
        IsDeleted = true;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTime.UtcNow;
    }
}
