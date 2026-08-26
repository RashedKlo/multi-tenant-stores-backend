using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
    {
        public void Configure(EntityTypeBuilder<CustomerAddress> builder)
        {
            builder.ToTable("customer_addresses", table =>
            {
                table.HasCheckConstraint(
                    "ck_customer_addresses_label_not_empty",
                    "length(btrim(label)) > 0");

                table.HasCheckConstraint(
                    "ck_customer_addresses_address_text_not_empty",
                    "length(btrim(address_text)) > 0");

                table.HasCheckConstraint(
                    "ck_customer_addresses_latitude_range",
                    "latitude BETWEEN -90 AND 90");

                table.HasCheckConstraint(
                    "ck_customer_addresses_longitude_range",
                    "longitude BETWEEN -180 AND 180");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.Label)
                .HasColumnName("label")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Latitude)
                .HasColumnName("latitude")
                .HasColumnType("decimal(10,7)")
                .IsRequired();

            builder.Property(x => x.Longitude)
                .HasColumnName("longitude")
                .HasColumnType("decimal(10,7)")
                .IsRequired();

            builder.Property(x => x.AddressText)
                .HasColumnName("address_text")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.IsDefault)
                .HasColumnName("is_default")
                .HasColumnType("boolean")
                .HasDefaultValue(false)
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

            // Unique (customer_id, id) – already implied by PK + FK, but kept for composite FK support
            builder.HasIndex(x => new { x.CustomerId, x.Id })
                .IsUnique()
                .HasDatabaseName("uq_customer_addresses_customer_id_id");

            builder.HasIndex(x => x.CustomerId)
                .HasDatabaseName("idx_customer_addresses_customer_id");

            // Partial unique: only one default address per customer (when not deleted)
            builder.HasIndex(x => x.CustomerId)
                .IsUnique()
                .HasFilter("is_default = true AND deleted_at IS NULL")
                .HasDatabaseName("uq_customer_addresses_one_default");

            builder.HasOne(x=>x.Customer)
                .WithMany(x=>x.Addresses)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}