using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class PurchaseOrderRepository
    : Repository<PurchaseOrder>,
      IPurchaseOrderRepository
{
    public PurchaseOrderRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }

    public override async Task<PurchaseOrder?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await Context.PurchaseOrders
            .Include(x => x.Supplier)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(
    CancellationToken cancellationToken = default)
    {
        return await Context.PurchaseOrders
            .Include(x => x.Supplier)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync(cancellationToken);
    }
}