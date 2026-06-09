using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class TrackWipConfiguration : IEntityTypeConfiguration<TrackWip>
{
    public void Configure(EntityTypeBuilder<TrackWip> builder)
    {
        builder.ToTable("TrackWips");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(t => t.WorkOrderId)
            .IsRequired();
        builder.Property(t => t.CurrentRouteId)
            .IsRequired();
        builder.Property(t => t.CurrentStepId)
            .IsRequired();
        builder.Property(t => t.CurrentWorkCenterId)
            .IsRequired();
        builder.Property(t => t.OperatorId)
            .IsRequired();
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(t => t.TrackInTime)
            .IsRequired();
        builder.Property(t => t.TrackOutTime)
            .IsRequired(false);

        builder.HasIndex(t => t.SerialNumber);
        builder.HasIndex(t => new { t.WorkOrderId, t.Status });
    }
}
