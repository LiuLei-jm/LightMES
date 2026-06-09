using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<WorkOrder> WorkOrders { get; }
    public DbSet<WorkOrderStepProgress> WorkOrderStepProgresses { get; }
    public DbSet<Route> Routes { get; }
    public DbSet<RouteStep> RouteSteps { get; }
    public DbSet<TrackWip> TrackWips { get; }
    public DbSet<User> Users { get; }
    public DbSet<Equipment> Equipments { get; }
    public DbSet<Material> Materials { get; }
    public DbSet<Role> Roles { get; }
    public DbSet<UserRole> UserRoles { get; }
    public DbSet<RolePermission> RolePermissions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
