using InventoryPlatform.Application.DTOs.Dashboard;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class DashboardRepository
    : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var totalProductsCount =
            await _context.Products
                .AsNoTracking()
                .CountAsync(cancellationToken);

        var activeProductsCount =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => x.IsActive,
                cancellationToken);

        var inactiveProductsCount =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => !x.IsActive,
                cancellationToken);

        var lowStockProductsCount =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => x.QuantityOnHand <= 10,
                cancellationToken);

        var outOfStockProductsCount =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => x.QuantityOnHand == 0,
                cancellationToken);

        var inventoryValue = await _context.Products
            .AsNoTracking()
            .Select(p => p.QuantityOnHand * p.CostPrice)
            .AsAsyncEnumerable()
            .DefaultIfEmpty(0)
            .SumAsync(cancellationToken);

        var recentTransactions =
            await _context.InventoryTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .OrderByDescending(t => t.TransactionDateUtc)
                .Take(10)
                .Select(t => new RecentTransactionDto
                {
                    Id = t.Id,
                    ProductName = t.Product.Name,
                    TransactionType = t.TransactionType.ToString(),
                    Quantity = t.Quantity,
                    TransactionDate = t.TransactionDateUtc
                })
                .ToListAsync(cancellationToken);

        var lowStockProducts =
            await _context.Products
                .AsNoTracking()
                .Where(p => p.QuantityOnHand <= 10)
                .OrderBy(p => p.QuantityOnHand)
                .Take(10)
                .Select(p => new LowStockProductDto
                {
                    Id = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.Category.Name,
                    QuantityOnHand = p.QuantityOnHand
                })
                .ToListAsync(cancellationToken);

        return new DashboardDto
        {
            Statistics = new DashboardStatisticsDto
            {
                TotalProducts = totalProductsCount,
                ActiveProducts = activeProductsCount,
                InactiveProducts = inactiveProductsCount,
                LowStockProducts = lowStockProductsCount,
                OutOfStockProducts = outOfStockProductsCount,
                InventoryValue = inventoryValue
            },

            RecentTransactions = recentTransactions,

            LowStockProducts = lowStockProducts
        };
    }
}
