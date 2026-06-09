using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<RouteStep> RouteSteps => Set<RouteStep>();
    public DbSet<TrackWip> TrackWips => Set<TrackWip>();
    public DbSet<User> Users => Set<User>();

    public DbSet<Equipment> Equipments => Set<Equipment>();

    public DbSet<Material> Materials => Set<Material>();

    public DbSet<WorkOrderStepProgress> WorkOrderStepProgresses => Set<WorkOrderStepProgress>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
