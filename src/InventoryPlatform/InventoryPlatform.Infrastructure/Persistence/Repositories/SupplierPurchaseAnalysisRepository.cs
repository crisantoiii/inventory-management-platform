using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class SupplierPurchaseAnalysisRepository
    : ISupplierPurchaseAnalysisRepository
{
    private readonly ApplicationDbContext _context;

    public SupplierPurchaseAnalysisRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SupplierPurchaseAnalysisDto>> GetSupplierPurchaseAnalysisAsync(
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

            purchaseOrders = purchaseOrders.Where(po =>
                po.Supplier.Name.Contains(search));
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

        var analysisQuery = purchaseOrders
            .GroupBy(po => new
            {
                po.SupplierId,
                SupplierName = po.Supplier.Name
            })
            .Select(group => new SupplierPurchaseAnalysisRow
            {
                SupplierId = group.Key.SupplierId,
                SupplierName = group.Key.SupplierName,

                FirstOrderDate = group.Min(
                    po => po.OrderDate),

                LastOrderDate = group.Max(
                    po => po.OrderDate),

                PurchaseOrderCount = group.Count(),

                TotalQuantity = group
                    .SelectMany(po => po.Items)
                    .Sum(item => item.Quantity),

                ReceivedQuantity = group
                    .SelectMany(po => po.Items)
                    .Sum(item => item.ReceivedQuantity),

                RemainingQuantity = group
                    .SelectMany(po => po.Items)
                    .Sum(item =>
                        item.Quantity - item.ReceivedQuantity),

                TotalAmount = group
                    .SelectMany(po => po.Items)
                    .Sum(item =>
                        item.Quantity * item.UnitCost)
            });

        var totalCount = await analysisQuery.CountAsync(
            cancellationToken);

        var orderedQuery = ApplySorting(
            analysisQuery,
            query);

        var rows = await orderedQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new SupplierPurchaseAnalysisDto(
                row.SupplierId,
                row.SupplierName,
                row.FirstOrderDate,
                row.LastOrderDate,
                row.PurchaseOrderCount,
                row.TotalQuantity,
                row.ReceivedQuantity,
                row.RemainingQuantity,
                row.TotalAmount))
            .ToList();

        return new PagedResult<SupplierPurchaseAnalysisDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<SupplierPurchaseAnalysisRow>
        ApplySorting(
            IQueryable<SupplierPurchaseAnalysisRow> query,
            PagedQuery request)
    {
        return request.SortBy switch
        {
            SupplierPurchaseAnalysisSortFields.Supplier =>
                request.Descending
                    ? query.OrderByDescending(
                        x => x.SupplierName)
                    : query.OrderBy(
                        x => x.SupplierName),

            SupplierPurchaseAnalysisSortFields.PurchaseOrderCount =>
                request.Descending
                    ? query.OrderByDescending(
                        x => x.PurchaseOrderCount)
                    : query.OrderBy(
                        x => x.PurchaseOrderCount),

            SupplierPurchaseAnalysisSortFields.TotalQuantity =>
                request.Descending
                    ? query.OrderByDescending(
                        x => x.TotalQuantity)
                    : query.OrderBy(
                        x => x.TotalQuantity),

            SupplierPurchaseAnalysisSortFields.ReceivedQuantity =>
                request.Descending
                    ? query.OrderByDescending(
                        x => x.ReceivedQuantity)
                    : query.OrderBy(
                        x => x.ReceivedQuantity),

            SupplierPurchaseAnalysisSortFields.RemainingQuantity =>
                request.Descending
                    ? query.OrderByDescending(
                        x => x.RemainingQuantity)
                    : query.OrderBy(
                        x => x.RemainingQuantity),

            SupplierPurchaseAnalysisSortFields.TotalAmount =>
                request.Descending
                    ? query.OrderByDescending(
                        x => x.TotalAmount)
                    : query.OrderBy(
                        x => x.TotalAmount),

            _ =>
                query
                    .OrderByDescending(
                        x => x.TotalAmount)
                    .ThenBy(
                        x => x.SupplierName)
        };
    }

    private sealed class SupplierPurchaseAnalysisRow
    {
        public int SupplierId { get; init; }

        public string SupplierName { get; init; } = string.Empty;

        public DateOnly FirstOrderDate { get; init; }

        public DateOnly LastOrderDate { get; init; }

        public int PurchaseOrderCount { get; init; }

        public decimal TotalQuantity { get; init; }

        public decimal ReceivedQuantity { get; init; }

        public decimal RemainingQuantity { get; init; }

        public decimal TotalAmount { get; init; }
    }
}