using LightMES.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightMES.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(u => u.EmployeeNo)
            .IsRequired()
            .HasMaxLength(30);
        builder.Property(u => u.BadgeNo)
            .HasMaxLength(50);
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(u => u.CreatedOn)
            .IsRequired();
        builder.Property(u => u.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(100);
        builder.Property(u => u.LastModifiedOn)
            .IsRequired(false);
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.EmployeeNo).IsUnique();
        builder.HasIndex(u => u.BadgeNo).HasFilter("\"BadgeNo\" IS NOT NULL");
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
