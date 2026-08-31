using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence.Configurations;

public class PendingOrderConfiguration : IEntityTypeConfiguration<PendingOrder>
{
    public void Configure(EntityTypeBuilder<PendingOrder> builder)
    {
        builder.ToTable("PendingOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(o => o.CreatedOnUtc).IsRequired();
    }
}
