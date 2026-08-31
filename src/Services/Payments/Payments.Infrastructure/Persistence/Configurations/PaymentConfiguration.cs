using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId).IsRequired();
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(p => p.ProcessedOnUtc).IsRequired();
        builder.Property(p => p.FailureReason).HasMaxLength(512);

        builder.HasIndex(p => p.OrderId).IsUnique();
    }
}
