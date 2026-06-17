using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.OrderNo).IsRequired().HasMaxLength(50);
        builder.Property(w => w.ProductCode).IsRequired().HasMaxLength(50);
        builder.Property(w => w.ProductName).IsRequired().HasMaxLength(100);
        builder.Property(w => w.PlanQty).IsRequired().HasDefaultValue(0);
        builder.Property(w => w.CompletedQty).IsRequired().HasDefaultValue(0);
        builder.Property(w => w.ScrapQty).IsRequired().HasDefaultValue(0);
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.RouteId).IsRequired();
        builder.Property(w => w.CurrentStepId).IsRequired(false);
        builder.Property(w => w.CreatedByUserId).IsRequired(false);
        builder.Property(w => w.CurrentOperatorId).IsRequired(false);
        builder.Property(w => w.PlannedStartTime).IsRequired();
        builder.Property(w => w.PlannedEndTime).IsRequired();
        builder.Property(w => w.ActualStartTime).IsRequired(false);
        builder.Property(w => w.ActualEndTime).IsRequired(false);

        builder.Property(w => w.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(w => w.CreatedOn).IsRequired();
        builder.Property(w => w.LastModifiedBy).IsRequired(false).HasMaxLength(100);
        builder.Property(w => w.LastModifiedOn).IsRequired(false);

        builder
            .HasOne(w => w.Route)
            .WithMany()
            .HasForeignKey(w => w.RouteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(w => w.CreatedByUser)
            .WithMany()
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(w => w.CurrentOperator)
            .WithMany()
            .HasForeignKey(w => w.CurrentOperatorId)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne(w => w.CurrentStep)
            .WithMany()
            .HasForeignKey(w => w.CurrentStepId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(w => w.OrderNo).IsUnique();
        builder.HasIndex(w => w.ProductCode);
        builder.HasQueryFilter(wo => !wo.IsDeleted);
    }
}
