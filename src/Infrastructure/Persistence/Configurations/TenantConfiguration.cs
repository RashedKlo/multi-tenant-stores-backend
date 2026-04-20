using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Email)
            .IsRequired();

        builder.Property(s => s.PasswordHash)
        .IsRequired();

        builder.Property(s => s.IsActive)
        .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
        .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.DeletedAt);




    }
}