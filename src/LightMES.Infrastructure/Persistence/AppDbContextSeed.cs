using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Common;
using LightMES.Domain.Constants;
using LightMES.Domain.Entities;

namespace LightMES.Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedDefaultDataAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher
    )
    {
        if (!context.Roles.Any())
        {
            var adminRole = new Role(SystemRoles.AdminId, SystemRoles.AdminName, "系统超级管理员");
            var supervisorRole = new Role(
                SystemRoles.SupervisorId,
                SystemRoles.SupervisorName,
                "车间班组长"
            );
            var operatorRole = new Role(
                SystemRoles.OperatorId,
                SystemRoles.OperatorName,
                "一线操作工"
            );
            var qcRole = new Role(SystemRoles.QCId, SystemRoles.QCName, "质检员");
            var plannerRole = new Role(
                SystemRoles.PlannerId,
                SystemRoles.PlannerName,
                "生产计划员"
            );
            context.Roles.AddRange(adminRole, supervisorRole, operatorRole, qcRole, plannerRole);
            var permissions = Permissions.GetAll();
            var adminPermissions = new List<RolePermission>();
            foreach (var permission in permissions)
            {
                adminPermissions.Add(
                    new RolePermission { RoleId = SystemRoles.AdminId, Permission = permission }
                );
            }
            context.RolePermissions.AddRange(adminPermissions);

            var passwordHash = passwordHasher.HashPassword("lightMES@666");
            var defaultAdmin = new User(
                Guid.NewGuid(),
                "admin",
                passwordHash,
                "Administrator",
                "Admin001",
                "Admin001",
                SystemConst.User.DefaultUser
            );
            await context.Users.AddAsync(defaultAdmin);
            var userRoleRelation = new UserRole { UserId = defaultAdmin.Id, RoleId = adminRole.Id };

            await context.UserRoles.AddAsync(userRoleRelation);

            await context.SaveChangesAsync();
        }
    }
}
