using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Sorting;
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
        string search = "",
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        PurchaseOrderStatus? status = null,
        string? sortBy = null,
        bool descending = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PurchaseOrder> query = Context.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Items);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            if (int.TryParse(search, out var purchaseOrderId))
            {
                query = query.Where(po =>
                    po.Id == purchaseOrderId ||
                    po.Supplier.Name.Contains(search));
            }
            else
            {
                query = query.Where(po =>
                    po.Supplier.Name.Contains(search));
            }
        }

        if (fromDate.HasValue)
        {
            query = query.Where(
                po => po.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(
                po => po.OrderDate <= toDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(
                po => po.Status == status.Value);
        }

        var orderedQuery = ApplySorting(
            query,
            sortBy,
            descending);

        return await orderedQuery.ToListAsync(cancellationToken);
    }

    private static IOrderedQueryable<PurchaseOrder> ApplySorting(
        IQueryable<PurchaseOrder> query,
        string? sortBy,
        bool descending)
    {
        return sortBy switch
        {
            PurchaseOrderSortFields.Id => descending
                ? query.OrderByDescending(po => po.Id)
                : query.OrderBy(po => po.Id),

            PurchaseOrderSortFields.Supplier => descending
                ? query.OrderByDescending(po => po.Supplier.Name)
                : query.OrderBy(po => po.Supplier.Name),

            PurchaseOrderSortFields.OrderDate => descending
                ? query.OrderByDescending(po => po.OrderDate)
                : query.OrderBy(po => po.OrderDate),

            PurchaseOrderSortFields.Status => descending
                ? query.OrderByDescending(po => po.Status)
                : query.OrderBy(po => po.Status),

            PurchaseOrderSortFields.TotalAmount => descending
                ? query.OrderByDescending(po => po.Items.Sum(item => item.Quantity * item.UnitCost))
                : query.OrderBy(po => po.Items.Sum(item => item.Quantity * item.UnitCost)),

            _ => query
                .OrderByDescending(po => po.OrderDate)
                .ThenByDescending(po => po.Id)
        };
    }
}
