using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class WorkOrderProcessConfiguation : IEntityTypeConfiguration<WorkOrderStepProgress>
{
    public void Configure(EntityTypeBuilder<WorkOrderStepProgress> builder)
    {
        builder.ToTable("WorkOrderStepProgresses");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.WorkOrderId)
            .IsRequired();
        builder.Property(p => p.StepId)
            .IsRequired();
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(p => p.EquipmentCode)
            .HasMaxLength(50)
            .IsRequired(false);
        builder.Property(p => p.PlannedQty)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(p => p.InQueueQty)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(p => p.ProcessingQty)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(p => p.GoodQty)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(p => p.DefectiveQty)
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(p => p.ScrapQty)
            .IsRequired()
            .HasDefaultValue(0);
        builder.HasIndex(p => new { p.WorkOrderId, p.StepId }).IsUnique();
    }
}
