using InventoryPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPlatform.Infrastructure.Persistence.Configurations;

public sealed class StockCountLineConfiguration : IEntityTypeConfiguration<StockCountLine>
{
    public void Configure(EntityTypeBuilder<StockCountLine> builder)
    {
        builder.ToTable("StockCountLines");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.StockCountSession)
            .WithMany(s => s.Lines)
            .HasForeignKey(x => x.StockCountSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InventoryTransaction)
            .WithMany()
            .HasForeignKey(x => x.InventoryTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ExpectedQuantity).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CountedQuantity).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.StockCountSessionId, x.ProductId }).IsUnique();
    }
}
