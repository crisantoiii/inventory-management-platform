using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
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

    public async Task<PagedResult<PurchaseOrder>> GetPurchaseOrdersAsync(
        PagedQuery query,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        PurchaseOrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PurchaseOrder> purchaseOrders = Context.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Items);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            if (int.TryParse(search, out var purchaseOrderId))
            {
                purchaseOrders = purchaseOrders.Where(po =>
                    po.Id == purchaseOrderId ||
                    po.Supplier.Name.Contains(search));
            }
            else
            {
                purchaseOrders = purchaseOrders.Where(po =>
                    po.Supplier.Name.Contains(search));
            }
        }

        if (fromDate.HasValue)
        {
            purchaseOrders = purchaseOrders.Where(
                po => po.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            purchaseOrders = purchaseOrders.Where(
                po => po.OrderDate <= toDate.Value);
        }

        if (status.HasValue)
        {
            purchaseOrders = purchaseOrders.Where(
                po => po.Status == status.Value);
        }

        var totalCount = await purchaseOrders.CountAsync(
            cancellationToken);

        var orderedQuery = ApplySorting(
            purchaseOrders,
            query);

        var items = await orderedQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PurchaseOrder>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<PurchaseOrder> ApplySorting(
        IQueryable<PurchaseOrder> query,
        PagedQuery request)
    {
        return request.SortBy switch
        {
            PurchaseOrderSortFields.Id => request.Descending
                ? query.OrderByDescending(po => po.Id)
                : query.OrderBy(po => po.Id),

            PurchaseOrderSortFields.Supplier => request.Descending
                ? query.OrderByDescending(po => po.Supplier.Name)
                : query.OrderBy(po => po.Supplier.Name),

            PurchaseOrderSortFields.OrderDate => request.Descending
                ? query.OrderByDescending(po => po.OrderDate)
                : query.OrderBy(po => po.OrderDate),

            PurchaseOrderSortFields.Status => request.Descending
                ? query.OrderByDescending(po => po.Status)
                : query.OrderBy(po => po.Status),

            PurchaseOrderSortFields.TotalAmount => request.Descending
                ? query.OrderByDescending(po => po.Items.Sum(item => item.Quantity * item.UnitCost))
                : query.OrderBy(po => po.Items.Sum(item => item.Quantity * item.UnitCost)),

            _ => query
                .OrderByDescending(po => po.OrderDate)
                .ThenByDescending(po => po.Id)
        };
    }
}
