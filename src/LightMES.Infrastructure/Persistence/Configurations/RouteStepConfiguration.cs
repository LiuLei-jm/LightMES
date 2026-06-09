using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class RouteStepConfiguration : IEntityTypeConfiguration<RouteStep>
{
    public void Configure(EntityTypeBuilder<RouteStep> builder)
    {
        builder.ToTable("RouteSteps");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.RouteId)
            .IsRequired();
        builder.Property(s => s.StepCode)
            .IsRequired()
            .HasMaxLength(30);
        builder.Property(s => s.StepName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(s => s.Sequence)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(s => s.StandardCycleTime)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(s => s.Description)
            .IsRequired(false)
            .HasMaxLength(500);
        builder.HasIndex(s => new { s.RouteId, s.Sequence }).IsUnique();
    }
}
