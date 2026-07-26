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
        var dashboard = new DashboardDto();

        var totalProducts =
            await _context.Products
                .AsNoTracking()
                .CountAsync(cancellationToken);

        var activeProducts =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => x.IsActive,
                cancellationToken);

        var inactiveProducts =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => !x.IsActive,
                cancellationToken);

        var lowStockProducts =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => x.QuantityOnHand <= 10,
                cancellationToken);

        var outOfStockProducts =
            await _context.Products
                .AsNoTracking()
                .CountAsync(
                x => x.QuantityOnHand == 0,
                cancellationToken);

        var inventoryValue =
            await _context.Products
                .AsNoTracking()
                .SumAsync(
                    x => x.QuantityOnHand * x.CostPrice,
                    cancellationToken);

        return new DashboardDto
        {
            Statistics = new DashboardStatisticsDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                InactiveProducts = inactiveProducts,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                InventoryValue = inventoryValue
            },

            RecentTransactions = [],

            LowStockProducts = []
        };
    }
}
