using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

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

            builder.Property(x => x.TokenHash)
                .HasColumnName("token_hash")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.RevokedAt)
                .HasColumnName("revoked_at")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            builder.Property(x => x.LastUsedAt)
                .HasColumnName("last_used_at")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => x.CustomerId)
                .HasDatabaseName("idx_refresh_tokens_customer_id");

            builder.HasIndex(x => x.TokenHash)
                .IsUnique()
                .HasDatabaseName("uq_refresh_tokens_token_hash");

            builder.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}