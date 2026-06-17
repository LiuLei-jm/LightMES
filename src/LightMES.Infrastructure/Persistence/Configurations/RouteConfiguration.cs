using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RouteCode).IsRequired().HasMaxLength(50);
        builder.Property(r => r.RouteName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Version).IsRequired().HasMaxLength(50);
        builder.Property(r => r.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(r => r.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(r => r.CreatedOn).IsRequired();
        builder.Property(r => r.LastModifiedBy).IsRequired(false);
        builder.Property(r => r.LastModifiedOn).IsRequired(false).HasMaxLength(100);

        builder.HasIndex(r => r.RouteCode).IsUnique();
        builder
            .HasMany(r => r.Steps)
            .WithOne()
            .HasForeignKey(s => s.RouteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
