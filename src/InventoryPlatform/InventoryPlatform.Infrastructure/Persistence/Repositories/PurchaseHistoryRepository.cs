using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class PurchaseHistoryRepository : IPurchaseHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PurchaseHistoryDto>> GetPurchaseHistoryAsync(
        PagedQuery query,
        DateOnly? fromDate,
        DateOnly? toDate,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PurchaseOrder> purchaseOrders = _context.PurchaseOrders
            .AsNoTracking();

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
            .Select(po => new PurchaseHistoryDto(
                po.Id,
                po.Supplier.Name,
                po.OrderDate,
                po.Status,
                po.Items.Sum(item =>
                    item.Quantity * item.UnitCost),
                po.Items.Sum(item =>
                    item.Quantity),
                po.Items.Sum(item =>
                    item.ReceivedQuantity),
                po.Items.Sum(item =>
                    item.Quantity - item.ReceivedQuantity)))
            .ToListAsync(cancellationToken);

        return new PagedResult<PurchaseHistoryDto>
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
            PurchaseHistorySortFields.OrderDate =>
                request.Descending
                    ? query.OrderByDescending(po => po.OrderDate)
                    : query.OrderBy(po => po.OrderDate),

            PurchaseHistorySortFields.Supplier =>
                request.Descending
                    ? query.OrderByDescending(po => po.Supplier.Name)
                    : query.OrderBy(po => po.Supplier.Name),

            PurchaseHistorySortFields.Status =>
                request.Descending
                    ? query.OrderByDescending(po => po.Status)
                    : query.OrderBy(po => po.Status),

            PurchaseHistorySortFields.TotalAmount =>
                request.Descending
                    ? query.OrderByDescending(po =>
                        po.Items.Sum(item =>
                            item.Quantity * item.UnitCost))
                    : query.OrderBy(po =>
                        po.Items.Sum(item =>
                            item.Quantity * item.UnitCost)),

            _ =>
                query
                    .OrderByDescending(po => po.OrderDate)
                    .ThenByDescending(po => po.Id)
        };
    }
}