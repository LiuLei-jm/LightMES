using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EquipmentCode)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(e => e.EquipmentName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(e => e.Location)
            .HasMaxLength(100)
            .IsRequired(false);
        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .IsRequired(false);
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.CreatedOn)
            .IsRequired();
        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(100)
            .IsRequired(false);
        builder.Property(e => e.LastModifiedBy)
            .IsRequired(false);

        builder.HasIndex(e => e.EquipmentCode)
            .IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
