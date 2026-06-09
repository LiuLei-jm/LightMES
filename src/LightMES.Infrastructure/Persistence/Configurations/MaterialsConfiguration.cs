using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class MaterialsConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MaterialCode)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(m => m.MaterialName)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(m => m.Specification)
            .HasMaxLength(200)
            .IsRequired(false);
        builder.Property(m => m.Unit)
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(m => m.MaterialType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(m => m.CreatedOn)
            .IsRequired();
        builder.Property(m => m.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(100);
        builder.Property(m => m.LastModifiedOn)
            .IsRequired(false);

        builder.HasIndex(m => m.MaterialCode)
            .IsUnique();
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
