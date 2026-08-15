using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class PurchaseHistoryRepository : IPurchaseHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PurchaseHistoryDto>> GetPurchaseHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .OrderByDescending(po => po.OrderDate)
            .ThenByDescending(po => po.Id)
            .Select(po => new PurchaseHistoryDto(
                po.Id,
                po.Supplier.Name,
                po.OrderDate,
                po.Status,
                po.Items.Sum(item => item.Quantity * item.UnitCost),
                po.Items.Sum(item => item.Quantity),
                po.Items.Sum(item => item.ReceivedQuantity),
                po.Items.Sum(item =>
                    item.Quantity - item.ReceivedQuantity)))
            .ToListAsync(cancellationToken);
    }
}