using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class GuestSessionConfiguration : IEntityTypeConfiguration<GuestSession>
    {
        public void Configure(EntityTypeBuilder<GuestSession> builder)
        {
            builder.ToTable("guest_sessions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.TokenHash)
                .HasColumnName("token_hash")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.LastSeenAt)
                .HasColumnName("last_seen_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasIndex(x => x.TokenHash)
                .IsUnique()
                .HasDatabaseName("uq_guest_sessions_token_hash");

            builder.HasIndex(x => x.ExpiresAt)
                .HasDatabaseName("idx_guest_sessions_expires_at");
        }
    }
}