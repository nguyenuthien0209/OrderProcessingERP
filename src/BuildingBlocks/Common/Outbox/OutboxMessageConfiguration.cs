using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).HasMaxLength(512).IsRequired();
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.OccurredOnUtc).IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2048);
    }
}
