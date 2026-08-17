using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customers", table =>
            {
                table.HasCheckConstraint(
                    "ck_customers_first_name_not_empty",
                    "length(btrim(first_name)) > 0");

                table.HasCheckConstraint(
                    "ck_customers_last_name_not_empty",
                    "length(btrim(last_name)) > 0");

                table.HasCheckConstraint(
                    "ck_customers_email_format",
                    "email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");

                table.HasCheckConstraint(
                    "ck_customers_auth_method",
                    "password_hash IS NOT NULL OR google_id IS NOT NULL");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.GoogleId)
                .HasColumnName("google_id")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired(false);

            builder.Property(x => x.IsEmailVerified)
                .HasColumnName("is_email_verified")
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("uq_customers_email");

            builder.HasIndex(x => x.GoogleId)
                .IsUnique()
                .HasFilter("google_id IS NOT NULL")
                .HasDatabaseName("uq_customers_google_id");

            builder.HasIndex(x => x.Email)
                .HasDatabaseName("idx_customers_email");
        }
    }
}