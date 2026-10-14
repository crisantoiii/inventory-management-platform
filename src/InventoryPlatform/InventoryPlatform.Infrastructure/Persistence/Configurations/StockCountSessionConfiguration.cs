using InventoryPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPlatform.Infrastructure.Persistence.Configurations;

public sealed class StockCountSessionConfiguration : IEntityTypeConfiguration<StockCountSession>
{
    public void Configure(EntityTypeBuilder<StockCountSession> builder)
    {
        builder.ToTable("StockCountSessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CountDate).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
