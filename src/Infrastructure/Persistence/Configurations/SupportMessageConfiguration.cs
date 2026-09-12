using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
{
    public void Configure(EntityTypeBuilder<SupportMessage> builder)
    {
        builder.ToTable("support_messages", table =>
        {
            table.HasCheckConstraint("ck_support_messages_body_not_empty", "length(btrim(body)) > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(x => x.ConversationId)
            .HasColumnName("conversation_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.SenderType)
            .HasColumnName("sender_type")
            .HasColumnType("support_sender_type")
            .IsRequired();

        builder.Property(x => x.SenderId)
            .HasColumnName("sender_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasColumnType("text")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.IsRead)
            .HasColumnName("is_read")
            .HasColumnType("boolean")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ConversationId, x.CreatedAt })
            .HasDatabaseName("idx_support_messages_conversation_created");
    }
}